using Augmenta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaVideoOutputFusionSpout : MonoBehaviour
{
	public AugmentaVideoOutput augmentaVideoOutput;

	private RenderTexture spoutTexture;

	private void OnEnable() {

		Klak.Spout.SpoutReceiver spoutReceiver = GetComponentInChildren<Klak.Spout.SpoutReceiver>();

		//Create spout texture
		spoutTexture = new RenderTexture(augmentaVideoOutput.videoOutputSizeInPixels.x, augmentaVideoOutput.videoOutputSizeInPixels.y, 0, RenderTextureFormat.ARGB32);
		//Assign texture to spout receiver
		spoutReceiver.targetTexture = spoutTexture;
		//Assign texture to spout display
		GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_MainTex", spoutTexture);

		//Set spout source name
		if (augmentaVideoOutput.autoFindFusionSpout) {
			foreach (var source in Klak.Spout.SpoutManager.GetSourceNames()) {
				if (source.Contains("Augmenta Fusion")) {
					spoutReceiver.sourceName = source;
					break;
				}
			}
		} else {
			spoutReceiver.sourceName = augmentaVideoOutput.fusionSpoutName;
		}
	}

	private void Update() {

		//Place spout display
		transform.position = augmentaVideoOutput.botLeftCorner + 0.5f * (augmentaVideoOutput.topLeftCorner - augmentaVideoOutput.botLeftCorner) + 0.5f * (augmentaVideoOutput.topRightCorner - augmentaVideoOutput.topLeftCorner);
		transform.localScale = new Vector3(Vector3.Distance(augmentaVideoOutput.botRightCorner, augmentaVideoOutput.botLeftCorner), Vector3.Distance(augmentaVideoOutput.topLeftCorner, augmentaVideoOutput.botLeftCorner), 1);

		Vector3 _videoOutputUp = (augmentaVideoOutput.topLeftCorner - augmentaVideoOutput.botLeftCorner).normalized;
		Vector3 _videoOutputForward = Vector3.Cross((augmentaVideoOutput.topRightCorner - augmentaVideoOutput.topLeftCorner).normalized, _videoOutputUp).normalized;

		transform.rotation = Quaternion.LookRotation(_videoOutputForward, _videoOutputUp);
	}

	private void OnDisable() {

		if (spoutTexture)
			spoutTexture.Release();
	}
}
