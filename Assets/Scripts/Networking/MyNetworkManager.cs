using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{

   //[SerializeField] private GameObject enemySpawnerPrefab = null;
   private Player player;
   //public GameObject cardAreaPrefab;
   //GameObject cardArea;


   public override void OnServerAddPlayer(NetworkConnectionToClient conn)
   {
        base.OnServerAddPlayer(conn);
        player = conn.identity.GetComponent<Player>();

        player.name = "Player " + numPlayers;

        //spawn the unitspawner at the player position
        /*GameObject cardAreaPrefabInstance = Instantiate(cardAreaPrefab
            conn.identity.transform.position, 
            conn.identity.transform.rotation);

        NetworkServer.Spawn(cardAreaPrefabInstance , conn);*/

   }

}
