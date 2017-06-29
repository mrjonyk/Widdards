using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class OverridenNetworkDiscovery : NetworkDiscovery {

	public override void OnReceivedBroadcast (string fromAddress, string data) {
		NetworkManager.singleton.networkAddress = fromAddress;
		NetworkManager.singleton.StartClient ();
		Debug.Log (fromAddress);
	}
}
