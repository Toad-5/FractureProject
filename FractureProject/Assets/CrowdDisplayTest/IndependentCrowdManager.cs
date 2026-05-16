using UnityEngine;
using System.Collections.Generic;

public class IndependentCrowdManager : MonoBehaviour
{
    private CrowdDisplayer.CharacterData[] cpuData;
    private CrowdNode referenceNode;
    private CrowdNode[] currentPathNodes; 
    private Crowd targetCrowd; 
    
    private ComputeBuffer crowdBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer waypointBuffer;
    private MaterialPropertyBlock propertyBlock;
    private Vector4[] waypointPositions; 
    
    private Mesh characterMesh;
    private Material materialTemplate;
    private Material materialInstance;
    
    private int currentWaypointCount; 
    private int characterCount;
    private float moveSpeed;
    private float totalPathLength;
    private float localOffset = 0f;

    private float dispersionDelay;
    private float dispersionDuration;
    private float dispersionDistance;
    private float blockedTimer = 0f;
    private float dispersionTimer = 0f;
    private bool isDispersing = false;

    public void Initialize(
        CrowdDisplayer.CharacterData[] characters, 
        Vector4[] pathWaypoints, 
        CrowdNode[] pathNodes, 
        int waypointCount, 
        float pathLength, 
        CrowdNode refNode, 
        Mesh mesh, 
        Material matTemplate, 
        Texture mainTex, 
        float speed,
        Crowd parentCrowd,
        float dispDelay,
        float dispDuration,
        float dispDist) 
    {
        cpuData = characters;
        characterCount = characters.Length;
        referenceNode = refNode;
        targetCrowd = parentCrowd;
        
        currentPathNodes = new CrowdNode[pathNodes.Length];
        System.Array.Copy(pathNodes, currentPathNodes, pathNodes.Length);
        
        waypointPositions = new Vector4[pathWaypoints.Length];
        System.Array.Copy(pathWaypoints, waypointPositions, pathWaypoints.Length);
        
        currentWaypointCount = waypointCount;
        totalPathLength = pathLength;
        
        characterMesh = mesh;
        materialTemplate = matTemplate;
        materialInstance = new Material(matTemplate);
        moveSpeed = speed;

        dispersionDelay = dispDelay;
        dispersionDuration = dispDuration;
        dispersionDistance = dispDist;

        propertyBlock = new MaterialPropertyBlock();
        if (mainTex != null) propertyBlock.SetTexture("_MainTex", mainTex);

        crowdBuffer = new ComputeBuffer(characterCount, 32);
        crowdBuffer.SetData(cpuData);
        propertyBlock.SetBuffer("_CrowdBuffer", crowdBuffer);

        waypointBuffer = new ComputeBuffer(waypointPositions.Length, 16);
        waypointBuffer.SetData(waypointPositions);
        propertyBlock.SetBuffer("_WaypointBuffer", waypointBuffer);

        propertyBlock.SetInt("_WaypointCount", currentWaypointCount);
        propertyBlock.SetFloat("_TotalPathLength", totalPathLength);
        
        propertyBlock.SetFloat("_DispersionProgress", 0f);
        propertyBlock.SetFloat("_DispersionDistance", dispersionDistance);

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(new uint[5] { characterMesh.GetIndexCount(0), (uint)characterCount, 0, 0, 0 });

        if (targetCrowd != null)
        {
            targetCrowd.OnCrowdPathChanged += UpdatePathData;
        }
    }

    private void UpdatePathData()
    {
        if (targetCrowd == null || currentPathNodes == null || currentPathNodes.Length == 0 || currentPathNodes[0] == null) return;

        int newWaypointCount = 0;
        float newAccumulatedDistance = 0f; 
        CrowdNode currentNode = currentPathNodes[0];
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
                if (newWaypointCount >= currentWaypointCount) {
                    diverged = true; 
                } else {
                    Vector3 oldPos = new Vector3(waypointPositions[newWaypointCount].x, waypointPositions[newWaypointCount].y, waypointPositions[newWaypointCount].z);
                    if (Vector3.Distance(oldPos, currentNode.position) > 0.01f) {
                        diverged = true; 
                        divergeIndex = newWaypointCount; 
                    } else {
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

        if (totalPathLength > 0f && commonPathLength < totalPathLength)
        {
            CrowdNode splitNode = null;
            if (divergeIndex >= 0 && divergeIndex < currentWaypointCount) {
                splitNode = currentPathNodes[divergeIndex]; 
            } else if (newWaypointCount < currentWaypointCount) {
                splitNode = currentPathNodes[newWaypointCount];
            }

            ExtractCutCharacters(totalPathLength, commonPathLength, splitNode);
            BakeOffsetAndRemoveCut(commonPathLength);
        }

        totalPathLength = newAccumulatedDistance;
        currentWaypointCount = newWaypointCount;

        for(int i = 0; i < currentWaypointCount; i++) {
            waypointPositions[i] = newWaypoints[i];
            currentPathNodes[i] = newPathNodes[i];
        }

        waypointBuffer.SetData(waypointPositions);
        propertyBlock.SetInt("_WaypointCount", currentWaypointCount);
        propertyBlock.SetFloat("_TotalPathLength", totalPathLength);
    }

    void ExtractCutCharacters(float oldLength, float cutLength, CrowdNode refNode)
    {
        if (refNode == null) return;

        crowdBuffer.GetData(cpuData);
        List<CrowdDisplayer.CharacterData> cutChars = new List<CrowdDisplayer.CharacterData>();

        for (int i = 0; i < characterCount; i++)
        {
            if (cpuData[i].uvRect.z == 0f) continue;

            float currentRealPos = cpuData[i].absoluteDistance + localOffset;
            if (cutLength < oldLength && currentRealPos > cutLength)
            {
                CrowdDisplayer.CharacterData copy = cpuData[i];
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

            GameObject go = new GameObject("IndependentCrowd_Cut_Sub");
            IndependentCrowdManager mgr = go.AddComponent<IndependentCrowdManager>();
            
            Texture tex = propertyBlock.GetTexture("_MainTex");
            mgr.Initialize(cutChars.ToArray(), oldPath, oldNodes, currentWaypointCount, oldLength, refNode, characterMesh, materialTemplate, tex, moveSpeed, targetCrowd, dispersionDelay, dispersionDuration, dispersionDistance);
        }
    }

    void BakeOffsetAndRemoveCut(float cutLength)
    {
        for (int i = 0; i < characterCount; i++) {
            if (cpuData[i].uvRect.z == 0f) continue;

            float currentRealPos = cpuData[i].absoluteDistance + localOffset;
            if (currentRealPos > cutLength) {
                cpuData[i].uvRect.z = 0f;
            } else {
                cpuData[i].absoluteDistance = currentRealPos;
            }
        }
        localOffset = 0f;
        crowdBuffer.SetData(cpuData);
    }

    void Update()
    {
        if (characterCount == 0) return;

        if (isDispersing)
        {
            dispersionTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(dispersionTimer / dispersionDuration);
            propertyBlock.SetFloat("_DispersionProgress", progress);

            Graphics.DrawMeshInstancedIndirect(characterMesh, 0, materialInstance, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer, 0, propertyBlock);

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
            return;
        }

        float maxCurrentPos = -float.MaxValue;
        int activeCount = 0;
        
        for (int i = 0; i < characterCount; i++)
        {
            if (cpuData[i].uvRect.z == 0f) continue;
            
            activeCount++;
            float currentPos = cpuData[i].absoluteDistance + localOffset;
            if (currentPos > maxCurrentPos) maxCurrentPos = currentPos;
        }

        if (activeCount == 0)
        {
            Destroy(gameObject);
            return;
        }

        bool isBlocked = false;
        for (int i = 0; i < currentWaypointCount; i++)
        {
            if (waypointPositions[i].w >= maxCurrentPos - 0.01f)
            {
                if (currentPathNodes[i] != null && currentPathNodes[i].state == CrowdState.Stagnant)
                {
                    isBlocked = true;
                }
                break; 
            }
        }

        bool canMove = true;
        bool bufferDirty = false;

        if (isBlocked)
        {
            float distanceToEnd = totalPathLength - maxCurrentPos;
            float step = Time.deltaTime * moveSpeed;

            canMove = !(step >= distanceToEnd);
        }

        if (canMove)
        {
            localOffset += Time.deltaTime * moveSpeed;
            
            blockedTimer = 0f;
        }
        else
        {
            blockedTimer += Time.deltaTime;
            if (blockedTimer >= dispersionDelay)
            {
                isDispersing = true;
                targetCrowd.RefreshCrowdStates(true);
                return; 
            }
        }

        for (int i = 0; i < characterCount; i++)
        {
            if (cpuData[i].uvRect.z != 0f) 
            {
                if (cpuData[i].absoluteDistance + localOffset > totalPathLength)
                {
                    cpuData[i].uvRect.z = 0f; 
                    bufferDirty = true;
                }
            }
        }

        if (bufferDirty) crowdBuffer.SetData(cpuData);

        propertyBlock.SetFloat("_GlobalOffset", localOffset);

        Graphics.DrawMeshInstancedIndirect(characterMesh, 0, materialInstance, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer, 0, propertyBlock);
    }

    void OnDisable()
    {
        if (targetCrowd != null) targetCrowd.OnCrowdPathChanged -= UpdatePathData;
        if (crowdBuffer != null) crowdBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
        if (waypointBuffer != null) waypointBuffer.Release();
        if (materialInstance != null) Destroy(materialInstance);
    }
}