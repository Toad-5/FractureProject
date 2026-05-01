using UnityEngine;

public class AlbumPhoto : MonoBehaviour
{
    bool Open,Close = true;
    Animator anim;
    public Player player;

    public void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        if(Input.GetKeyDown("e") || Input.GetButtonDown("Fire3"))
        {
            if (player && player.locked) return;
            if (!Open)
            {
                Open = true;
                Close = false;
                anim.SetTrigger("Open");
                if (player)
                {
                    player.LockPlayer(true);
                }
            }
        }

        if (!Input.GetKey("e") && !Input.GetButton("Fire3"))
        {
            if (!Close)
            {
                Open = false;
                Close = true;
                anim.SetTrigger("Close");
                if (player)
                {
                    player.LockPlayer(false);
                }
            }
        }
    }
}
