using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using System;

public class ChatBehaviour: NetworkBehaviour
{
    [SerializeField] private GameObject chatUI = null;
    [SerializeField] private TMP_Text chatText = null;
    [SerializeField] private TMP_InputField inputField = null;
    private Player player;

    //Ich kann versuchen die message die gesendet wird auch als syncvar zu machen

    //Die chat history
    [SyncVar] [SerializeField] private string chatHistory;

    private static event Action<string> OnMessage;

    public override void OnStartAuthority()
    {
        player = GetComponent<Player>();
        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if(!hasAuthority) {return;}
        OnMessage -= HandleNewMessage;
    }

    [Command]
    private void CmdAddMessage(string message)
    {
        chatHistory += message + '\n';
        Debug.Log("chathistory is: " + chatHistory);
    }

    //[Client]
    private void HandleNewMessage(String message)
    {
        chatText.text = chatHistory;
    }

    [Client]
    public void Send(string message)
    {
        if(!Input.GetKeyDown(KeyCode.Return)) {return;}

        if(string.IsNullOrWhiteSpace(message)) {return;}

        CmdSendMessage(inputField.text);

        //CmdAddMessage(message);

        inputField.text = string.Empty;
    }

    //Server sends message to all clients
    [Command]
    private void CmdSendMessage(string message)
    {
        RpcHandleMessage($"[{connectionToClient.connectionId}]: {message}");
        //RpcHandleMessage($"[{player.Name}]: {message}"); <- PROBLEM: DAS HIER FUNZT NET
    }

    [ClientRpc]
    private void RpcHandleMessage(String message)
    {
        if(hasAuthority)
            CmdAddMessage(message);//Add message to the chattext
        OnMessage?.Invoke($"\n{message}");
    }

    /*TODO: 
        use syncvar instead of clientrpc
        Server stores message history

    */


}



