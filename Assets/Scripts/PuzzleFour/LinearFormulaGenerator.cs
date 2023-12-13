using UnityEngine;

public class LinearFormulaGenerator : MonoBehaviour {
    [SerializeField]
    private int minValue = -10;

    [SerializeField]
    private int maxValue = 10;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private Vector2 GenerateAAndB() {
        int valueA = Random.Range(minValue, maxValue);
        int valueB = Random.Range(minValue, maxValue);

        Vector2 returnValue = new Vector2(valueA, valueB);

        return returnValue;
    }

    private string BuildFormulaFromAAndB(int valueA, int valueB) {
        string returnValue = valueA.ToString() + "x";
        if(valueB < 0) {
            returnValue = returnValue + " - " + (valueB * -1).ToString();
        } else {
            returnValue = returnValue + " + " + valueB.ToString();
        }

        return returnValue;
    }

    /*private int[] XPointsOnGraph() {
        int[] xPoints;



        return xPoints;
    }*/
}