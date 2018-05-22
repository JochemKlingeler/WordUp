using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckPoint : MonoBehaviour {

    public GameMaster gameMaster;
    public Player player;
    public int checkpointNumber = 0;
    public int lettersNeeded;
    string amountText;
    public string message = "";
    public GameObject messageObject;  

    // Use this for initialization
    void Start()
    {
        message = message.Replace("\\n", "\n");
        gameMaster = FindObjectOfType<GameMaster>();
        player = FindObjectOfType<Player>();
        messageObject = transform.Find("CheckpointMessage").gameObject;
    }

    void Update()
    {
        if(gameMaster.currentCheckPoint != gameObject)
        gameObject.GetComponent<Animator>().SetBool("checked", false);
    }

	// Update is called once per frame
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
			gameObject.GetComponent<Animator> ().SetBool ("checked", true);
            gameMaster.currentCheckPoint = gameObject;

            //if current amount of letters is lower than the expected amount of letters, show message stating the amount of missing letters
            if (player.countLetters < lettersNeeded)
            {

                int verschilLetters = (lettersNeeded - player.countLetters);

                if (verschilLetters != 1)
                {
                    amountText = verschilLetters.ToString();
                    message = amountText + " Letters gemist...";
                    messageObject.GetComponent<TextMesh>().text = message;
                }
                else
                {
                    amountText = verschilLetters.ToString();
                    message = amountText + " Letter gemist...";
                    messageObject.GetComponent<TextMesh>().text = message;
                }

            }
            else
            {
                message = "";
                messageObject.GetComponent<TextMesh>().text = message;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            message = "";
            messageObject.GetComponent<TextMesh>().text = message;
        }
    }
}
