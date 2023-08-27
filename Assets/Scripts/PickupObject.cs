using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PickupObject : NetworkBehaviour
{
    public Transform objectHolder; // The Transform that represents the position of the object holder
    public LayerMask pickupLayer; // The layer of the objects that can be picked up
    public LayerMask snapLayer; // The layer of snapping points
    public Camera cam;

    [SerializeField]
    private NetworkObject currentObject; // The current picked-up object

    private Image crosshairImage; // Referentie naar de image component van de crosshair

    private void Start() {
        crosshairImage = GameObject.FindWithTag("Crosshair").GetComponent<Image>();
    }

	private void Update()
    {
        if (!IsLocalPlayer)
            return;

        // Raycast to select and pick up an object
        Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.red, 100f);
        RaycastHit hit;

        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, pickupLayer)) {
            crosshairImage.color = Color.blue;
        } else if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, snapLayer)) {
            crosshairImage.color = Color.yellow;
        } else {
            crosshairImage.color = Color.white;
        }

        // Check if the player presses the pickup button
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("I CLICKED E");
            if (currentObject == null)
            {
                Debug.Log("CURRENT OBJECT IS 0");
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, pickupLayer))
                {
                    // Send an RPC to the server to pick up the object
                    PickUpObjectServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                    Debug.Log("PICKED UP");
                }
            }
            else
            {
                if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, snapLayer)) {
                    SnapObjectServerRpc(hit.transform.position, hit.transform.gameObject.GetComponent<MeshRenderer>());
                } else {
                    // Send an RPC to the server to drop the object
                    DropObjectServerRpc();
                }
            }
        }

        if (currentObject != null)
        {
            // Update the position and rotation of the held object to match the object holder
            currentObject.transform.position = objectHolder.position;
            currentObject.transform.rotation = objectHolder.rotation;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickUpObjectServerRpc(ulong objectId)
    {
        if (currentObject != null)
            return;

        // Check if the dictionary contains the key
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject pickedObject))
        {
            // Mark the object as picked up
            currentObject = pickedObject;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;

            // Send an RPC to all clients to synchronize the changes in the picked-up object
            PickUpObjectClientRpc(currentObject.NetworkObjectId);
        }
        else
        {
            Debug.LogError($"Failed to find object with NetworkObjectId: {objectId}");
        }
    }

    [ClientRpc]
    private void PickUpObjectClientRpc(ulong objectId)
    {
        // Mark the object as picked up
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject obj))
        {
            currentObject = obj;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropObjectServerRpc() {
        if (currentObject == null)
            return;

        // Unmark the object as picked up and let it drop
        currentObject.GetComponent<Rigidbody>().isKinematic = false;

        // Send an RPC to all clients to synchronize the changes in the picked-up object
        DropObjectClientRpc(currentObject.NetworkObjectId);

        currentObject = null;
    }

    [ClientRpc]
    private void DropObjectClientRpc(ulong objectId)
    {
        // Unmark the object as picked up and let it drop
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject obj))
        {
            obj.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SnapObjectServerRpc(Vector3 snapPointTransform, MeshRenderer snapPointRenderer) {
        if(currentObject == null)
            return;

        // Unmark the object as picked up and let it drop
        currentObject.transform.position = snapPointTransform;
        currentObject.transform.rotation = Quaternion.Euler(270f, 0f, 0f);

        if(true /*hierin nog checken of de oplossing klopt*/) {
            snapPointRenderer.forceRenderingOff = true;
        }

        // Send an RPC to all clients to synchronize the changes in the picked-up object
        SnapObjectClientRpc(snapPointTransform, snapPointRenderer, currentObject.NetworkObjectId);

        currentObject = null;
    }

    [ClientRpc]
    private void SnapObjectClientRpc(Vector3 snapPointTransform, MeshRenderer snapPointRenderer, ulong objectId) {
        // Unmark the object as picked up and let it drop
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject obj)) {
            obj.transform.position = snapPointTransform;
            currentObject.transform.rotation = Quaternion.Euler(270f, 0f, 0f);

            if(true /*hierin nog checken of de oplossing klopt*/) {
                snapPointRenderer.forceRenderingOff = true;
            }
        }
    }
}