using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class GearPuzzleController : NetworkBehaviour {
    public enum PuzzleOptions {
        Option1,
        Blabla,
        Option55
    }

    [SerializeField]
    private int wantedSolutions;

    private int amountOfSolved;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private PuzzleOptions selectedPuzzle;

    [SerializeField]
    private GameObject smallGearPrefab;

	[SerializeField]
	private GameObject bigGearPrefab;

    [SerializeField]
    private List<Transform> smallGearSpawnPoints = new List<Transform>();

    private FormulaGenerator formulaGenerator;
    private Dictionary<string, float[]> formulasAndSolutionsControllerCopy;
    private int[] solutionsCopy;
    private List<GameObject> spawnedGears = new List<GameObject>();

    [SerializeField]
    private GameObject snapPointGear1;

    [SerializeField]
    private GameObject snapPointGear2;

    [SerializeField]
    private GameObject snapPointGear3;

    [SerializeField]
    private GameObject snapPointGear4;

    [SerializeField]
    private GameObject snapPointGear5;

    // Dictionary om snap points te koppelen aan formules
    private Dictionary<GameObject, string> snapPointFormulas = new Dictionary<GameObject, string>();

    public PuzzleOptions GetSelectedPuzzle() {
        return selectedPuzzle;
    }

    [SerializeField] private List<GameObject> objectsToActivate = new List<GameObject>();

    public NetworkVariable<string> TMPText = new NetworkVariable<string>();

    private TextMeshProUGUI tmp;

    // Start is called before the first frame update
    private void Start() {
        amountOfSolved = 0;
        wantedSolutions = 5;
        Debug.Log("Selected option: " + selectedPuzzle);

        // Haal het FormulaGenerator-script op via GetComponent
        formulaGenerator = GetComponent<FormulaGenerator>();

        // Controleer of het FormulaGenerator-script is gevonden
        if(formulaGenerator != null) {
            // Roep GenerateFormulas aan via de referentie naar FormulaGenerator
            formulaGenerator.GenerateFormulas();
        } else {
            Debug.LogError("FormulaGenerator component not found!");
        }
        SpawnGears();

        tmp = GetComponent<TextMeshProUGUI>();

        // Listen for changes in the TMP text value
        TMPText.OnValueChanged += OnTMPTextChanged;
    }

	private void Update() {
        if(amountOfSolved == wantedSolutions) {
            foreach(GameObject objectToActivate in objectsToActivate) {
                objectToActivate.GetComponent<InteractableMechanism>().Activate();
            }
            amountOfSolved = 0;
        }
	}

	// Deze methode controleert of een snap point de juiste formule heeft
	public bool CheckSnapPointFormula(GameObject snapPoint, string formula) {
        if(snapPointFormulas.ContainsKey(snapPoint)) {
            string correctFormula = snapPointFormulas[snapPoint];
            
            if(formula == correctFormula) {
                amountOfSolved++;
                Debug.Log("aantal opgelost: " + amountOfSolved);
                return true;
            } else {
                Debug.Log("aantal opgelost: " + amountOfSolved);
                return false;
            }
        }
        Debug.Log("aantal opgelost: " + amountOfSolved);
        return false;
    }

    public void AddSnapPointFormula() {
        snapPointFormulas.Add(snapPointGear1, formulasAndSolutionsControllerCopy.ElementAt(0).Key);
        snapPointFormulas.Add(snapPointGear2, formulasAndSolutionsControllerCopy.ElementAt(1).Key);
        snapPointFormulas.Add(snapPointGear3, formulasAndSolutionsControllerCopy.ElementAt(2).Key);
        snapPointFormulas.Add(snapPointGear4, formulasAndSolutionsControllerCopy.ElementAt(3).Key);
        snapPointFormulas.Add(snapPointGear5, formulasAndSolutionsControllerCopy.ElementAt(4).Key);
    }

    public void SpawnGears() {
        formulasAndSolutionsControllerCopy = GetComponent<FormulaGenerator>().GetFormulasAndSolutions();
        solutionsCopy = GetComponent<FormulaGenerator>().GetSolutions();
        Debug.Log("Am I client? " + IsClient + "Am I host? " + IsHost + "Am I server? " + IsServer);

        AddSnapPointFormula();

        if(formulasAndSolutionsControllerCopy == null) {
            Debug.LogError("Formulas and solutions not generated!");
            return;
        }

        // Loop om het gewenste aantal tandwielen te spawnen
        for(int i = 0; i < 5; i++) {
            // Pas de positie van het bigGearPrefab aan naar de positie van het gameobject
            Vector3 spawnPosition = spawnPoint.position;

            // Maak een instantie van het bigGearPrefab op de aangepaste positie

            GameObject newBigGear = Instantiate(bigGearPrefab, spawnPosition, Quaternion.identity);
            newBigGear.GetComponent<NetworkObject>().Spawn();

            Debug.Log(newBigGear.GetComponent<NetworkObject>().OwnerClientId);

            if(!newBigGear.GetComponent<NetworkObject>().IsSpawned) {
                Debug.Log("NetworkObject is not spawned or has been destroyed.");
            }

            // Haal de TextMeshProUGUI-component op van newBigGear
            TextMeshProUGUI tmp = newBigGear.GetComponentInChildren<TextMeshProUGUI>();

            // Controleer of er een TMP-component is gevonden
            if(tmp != null) {
                // Controleer of het huidige indexnummer binnen de geldige bereik ligt
                if(i < formulasAndSolutionsControllerCopy.Count) {
                    // Haal de formule op uit de dictionary
                    KeyValuePair<string, float[]> formulaEntry = formulasAndSolutionsControllerCopy.ElementAt(i);
                    string formula = formulaEntry.Key;

                    // Pas de formule toe op de tekst van het TMP-object
                    tmp.text = formula;
                } else {
                    Debug.LogWarning("No formula found for big gear at index: " + i);
                }
            } else {
                Debug.LogError("TextMeshPro component not found on big gear!");
            }

            // Optioneel: Pas de positie en rotatie van newBigGear aan naar wens

            // Voeg newBigGear toe aan de lijst met gespawnede tandwielen
            spawnedGears.Add(newBigGear);
        }

        // Loop om het gewenste aantal tandwielen te spawnen
        for(int j = 0; j < 6; j++) {
            // Pas de positie van het smallGearPrefab aan naar de positie van het gameobject
            Vector3 spawnPosition = smallGearSpawnPoints[j].position;

            // Maak een instantie van het smallGearPrefab op de aangepaste positie
            GameObject newSmallGear = Instantiate(smallGearPrefab, spawnPosition, Quaternion.identity);
            newSmallGear.GetComponent<NetworkObject>().Spawn();

            // Pas de rotatie aan om de kleine gears verticaal te laten staan
            newSmallGear.transform.rotation = Quaternion.Euler(270f, 0f, 0f);

            // Haal de TextMeshProUGUI-component op van newSmallGear
            TextMeshProUGUI tmp = newSmallGear.GetComponentInChildren<TextMeshProUGUI>();

            // Controleer of er een TMP-component is gevonden
            if(tmp != null) {
                // Controleer of het huidige indexnummer binnen de geldige bereik ligt
                if(j < solutionsCopy.Length) {
                    // Haal de formule op uit de dictionary
                    int solutionOnGear = solutionsCopy[j];

                    // Pas de formule toe op de tekst van het TMP-object
                    tmp.text = solutionOnGear.ToString();
                } else {
                    Debug.LogWarning("No solution found for small gear at index: " + j);
                }
            } else {
                Debug.LogError("TextMeshPro component not found on big gear!");
            }
        }
    }

    private void OnTMPTextChanged(string oldValue, string newValue) {
        // Update the TMP component with the new text value
        tmp.text = newValue;

        // Call the ServerRpc to synchronize the TMP text across the network
        UpdateTMPTextServerRpc(newValue);
    }

    [ServerRpc (RequireOwnership = false)]
    private void UpdateTMPTextServerRpc(string newText) {
        // Update the TMP text on the server
        TMPText.Value = newText;

        // Call the ClientRpc to synchronize the TMP text with the clients
        UpdateTMPTextClientRpc(newText);
    }

    [ClientRpc]
    private void UpdateTMPTextClientRpc(string newText) {
        // Update the TMP text on the clients
        TMPText.Value = newText;
    }
}