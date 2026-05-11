using UnityEngine;
using UnityEngine.U2D;

public class CrowdDisplayer : MonoBehaviour
{
    public struct CharacterData {
        public Vector3 randomOffset;
        public float absoluteDistance;
        public Vector4 uvRect;
    }

    public Crowd targetCrowd; 
    public SpriteAtlas atlas;
    public Mesh characterMesh; 
    public Material crowdMaterialTemplate; 
    
    public int characterCount;
    public float moveSpeed; 
    public float catchUpSpeed;
    public float characterRotationY = 0f;
    
    private ComputeBuffer crowdBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer waypointBuffer; 
    private Vector4[] waypointPositions;
    private CrowdNode[] currentPathNodes;
    
    private float globalOffset = 0f;
    private MaterialPropertyBlock propertyBlock;
    
    private Sprite[] characters;
    private int currentWaypointCount = 0;
    
    private float currentPathLength = 0f; 
    private CharacterData[] cpuData;
    private bool hasStartedLooping = false;

    void Start()
    {
        if (targetCrowd == null) return;
        InitializeCrowd();
        targetCrowd.OnCrowdPathChanged += UpdatePathData;
        UpdatePathData();
    }

    void InitializeCrowd()
    {
        if (atlas != null) { characters = new Sprite[atlas.spriteCount]; atlas.GetSprites(characters); }
        
        propertyBlock = new MaterialPropertyBlock();
        float initialLength = CalculateInitialPathLength();
        cpuData = new CharacterData[characterCount];

        for (int i = 0; i < characterCount; i++)
        {
            Sprite s = characters[Random.Range(0, characters.Length)];
            Rect r = s.textureRect;
            cpuData[i] = new CharacterData {
                randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 10f), 0),
                absoluteDistance = Random.value * initialLength,
                uvRect = new Vector4(r.x / s.texture.width, r.y / s.texture.height, r.width / s.texture.width, r.height / s.texture.height)
            };
        }

        crowdBuffer = new ComputeBuffer(characterCount, 32);
        crowdBuffer.SetData(cpuData);
        propertyBlock.SetBuffer("_CrowdBuffer", crowdBuffer);
        if (characters.Length > 0 && characters[0] != null) propertyBlock.SetTexture("_MainTex", characters[0].texture);

        int maxPossibleNodes = targetCrowd.allNodes.Length;
        waypointPositions = new Vector4[maxPossibleNodes];
        currentPathNodes = new CrowdNode[maxPossibleNodes];
        waypointBuffer = new ComputeBuffer(maxPossibleNodes, 16); 
        propertyBlock.SetBuffer("_WaypointBuffer", waypointBuffer);

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(new uint[5] { characterMesh.GetIndexCount(0), (uint)characterCount, 0, 0, 0 });
    }

    private void UpdatePathData()
    {
        if (targetCrowd == null || targetCrowd.rootNode == null) return;

        int newWaypointCount = 0;
        float newAccumulatedDistance = 0f; 
        CrowdNode currentNode = targetCrowd.rootNode; 
        Vector3 lastPosition = currentNode.position;
        
        Vector4[] newWaypoints = new Vector4[waypointPositions.Length];
        CrowdNode[] newPathNodes = new CrowdNode[waypointPositions.Length];
        
        float commonPathLength = 0f;
        bool diverged = false;
        int divergeIndex = -1;

        while (currentNode != null)
        {
            newAccumulatedDistance += Vector3.Distance(lastPosition, currentNode.position);
            newWaypoints[newWaypointCount] = new Vector4(currentNode.position.x, currentNode.position.y, currentNode.position.z, newAccumulatedDistance);
            newPathNodes[newWaypointCount] = currentNode;
            
            if (!diverged)
            {
                if (newWaypointCount >= currentWaypointCount) 
                {
                    diverged = true; 
                } 
                else 
                {
                    Vector3 oldPos = new Vector3(waypointPositions[newWaypointCount].x, waypointPositions[newWaypointCount].y, waypointPositions[newWaypointCount].z);
                    if (Vector3.Distance(oldPos, currentNode.position) > 0.01f) 
                    {
                        diverged = true; 
                        divergeIndex = newWaypointCount; 
                    } 
                    else 
                    {
                        commonPathLength = newAccumulatedDistance; 
                    }
                }
            }
            
            lastPosition = currentNode.position;
            newWaypointCount++;
            
            if (newWaypointCount >= newWaypoints.Length) break;
            currentNode = currentNode.nextNode;
        }
        
        if (newWaypointCount < 2) return;

        if (currentPathLength > 0f && commonPathLength < currentPathLength)
        {
            CrowdNode splitNode = null;
            if (divergeIndex >= 0 && divergeIndex < currentWaypointCount) 
            {
                splitNode = currentPathNodes[divergeIndex]; 
            } 
            else if (newWaypointCount < currentWaypointCount) 
            {
                splitNode = currentPathNodes[newWaypointCount];
            }

            ExtractCutCharacters(currentPathLength, commonPathLength, splitNode);
            RescaleAbsoluteDistances(currentPathLength, commonPathLength, newAccumulatedDistance);
            hasStartedLooping = false; 
        }
        else if (newAccumulatedDistance > currentPathLength + 0.01f)
        {
            hasStartedLooping = false;
        }

        currentPathLength = newAccumulatedDistance;
        currentWaypointCount = newWaypointCount;

        for (int i = 0; i < currentWaypointCount; i++) 
        {
            waypointPositions[i] = newWaypoints[i];
            currentPathNodes[i] = newPathNodes[i];
        }

        waypointBuffer.SetData(waypointPositions);
        propertyBlock.SetInt("_WaypointCount", currentWaypointCount);
        propertyBlock.SetFloat("_TotalPathLength", currentPathLength);
    }
    
    void ExtractCutCharacters(float oldLength, float cutLength, CrowdNode refNode)
    {
        if (refNode == null) return;

        crowdBuffer.GetData(cpuData);
        System.Collections.Generic.List<CharacterData> cutChars = new System.Collections.Generic.List<CharacterData>();

        for (int i = 0; i < characterCount; i++)
        {
            float currentRealPos = cpuData[i].absoluteDistance + globalOffset;
            
            if (cutLength < oldLength && currentRealPos > cutLength)
            {
                CharacterData copy = cpuData[i];
                copy.absoluteDistance = currentRealPos; 
                cutChars.Add(copy);
            }
        }

        if (cutChars.Count > 0)
        {
            Vector4[] oldPath = new Vector4[currentWaypointCount];
            System.Array.Copy(waypointPositions, oldPath, currentWaypointCount);

            CrowdNode[] oldNodes = new CrowdNode[currentWaypointCount];
            System.Array.Copy(currentPathNodes, oldNodes, currentWaypointCount);

            GameObject go = new GameObject("IndependentCrowd_Cut");
            IndependentCrowdManager mgr = go.AddComponent<IndependentCrowdManager>();
            
            Texture tex = (characters != null && characters.Length > 0) ? characters[0].texture : null;
            
            mgr.Initialize(cutChars.ToArray(), oldPath, oldNodes, currentWaypointCount, oldLength, refNode, characterMesh, crowdMaterialTemplate, tex, catchUpSpeed, targetCrowd);
        }
    }

    void Update()
    {
        if (propertyBlock == null || crowdMaterialTemplate == null || targetCrowd.rootNode == null || currentWaypointCount < 2) return;

        bool isFlowing = targetCrowd.rootNode.state == CrowdState.Flowing;
        float currentSpeed = (isFlowing && hasStartedLooping) ? moveSpeed : catchUpSpeed;
        
        bool canMove = true;
        bool bufferDirty = false;

        if (!isFlowing) 
        {
            float maxCurrentPos = -float.MaxValue;
            for (int i = 0; i < characterCount; i++) {
                float currentPos = cpuData[i].absoluteDistance + globalOffset;
                if (currentPos > maxCurrentPos) maxCurrentPos = currentPos;
            }
            float distanceToEnd = currentPathLength - maxCurrentPos;
            float step = Time.deltaTime * currentSpeed; 
            canMove = !(step >= distanceToEnd);

            if (!canMove && distanceToEnd > 0) globalOffset += distanceToEnd;
        }

        if (canMove) globalOffset += Time.deltaTime * currentSpeed; 

        if (isFlowing)
        {
            float averageSpacing = currentPathLength / Mathf.Max(1, characterCount);
            float minRealPos = float.MaxValue;
            
            for (int j = 0; j < characterCount; j++) {
                float realPos = cpuData[j].absoluteDistance + globalOffset;
                if (realPos < minRealPos) minRealPos = realPos;
            }

            for (int i = 0; i < characterCount; i++) {
                float currentRealPos = cpuData[i].absoluteDistance + globalOffset;
                if (currentRealPos > currentPathLength) {
                    hasStartedLooping = true;
                    float newRealPos = minRealPos - averageSpacing;
                    cpuData[i].absoluteDistance = newRealPos - globalOffset;
                    minRealPos = newRealPos; 
                    bufferDirty = true;
                }
            }
        }

        if (bufferDirty) NormalizeDistances();

        propertyBlock.SetFloat("_GlobalOffset", globalOffset);
        propertyBlock.SetFloat("_RotationY", characterRotationY);
        
        Graphics.DrawMeshInstancedIndirect(characterMesh, 0, crowdMaterialTemplate, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer, 0, propertyBlock);
    }
    
    void RescaleAbsoluteDistances(float oldLength, float cutLength, float trueNewLength)
    {
        crowdBuffer.GetData(cpuData);
        float averageSpacing = trueNewLength / Mathf.Max(1, characterCount);
        float minRealPos = 0f; 
        bool hasKeptCharacters = false;

        for (int i = 0; i < characterCount; i++) {
            float currentRealPos = cpuData[i].absoluteDistance + globalOffset;
            if (!(cutLength < oldLength && currentRealPos > cutLength)) {
                if (!hasKeptCharacters || currentRealPos < minRealPos) {
                    minRealPos = currentRealPos;
                    hasKeptCharacters = true;
                }
            }
        }

        for (int i = 0; i < characterCount; i++) {
            float currentRealPos = cpuData[i].absoluteDistance + globalOffset;
            if (cutLength < oldLength && currentRealPos > cutLength) {
                minRealPos -= averageSpacing;
                cpuData[i].absoluteDistance = minRealPos; 
            } else {
                cpuData[i].absoluteDistance = currentRealPos;
            }
        }
        
        globalOffset = 0f;
        crowdBuffer.SetData(cpuData);
    }

    void NormalizeDistances()
    {
        float minDistance = float.MaxValue;
        for (int i = 0; i < characterCount; i++) {
            if (cpuData[i].absoluteDistance < minDistance) minDistance = cpuData[i].absoluteDistance;
        }

        if (minDistance > 0f) {
            for (int i = 0; i < characterCount; i++) cpuData[i].absoluteDistance -= minDistance;
            globalOffset += minDistance;
        }
        crowdBuffer.SetData(cpuData);
    }

    float CalculateInitialPathLength()
    {
        float total = 0f;
        CrowdNode current = targetCrowd.rootNode;
        Vector3 last = current.position;
        while (current != null) {
            total += Vector3.Distance(last, current.position);
            last = current.position;
            current = current.nextNode;
        }
        return total;
    }

    void OnDisable() 
    {
        if (targetCrowd != null) targetCrowd.OnCrowdPathChanged -= UpdatePathData;
        if (crowdBuffer != null) crowdBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
        if (waypointBuffer != null) waypointBuffer.Release(); 
    }
}