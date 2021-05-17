using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HexCell : MonoBehaviour
{
    ////////////////////////////////

    public enum HexDirection
    {
        N, NE, SE, S, SW, NW
    }

    ////////////////////////////////

    [SerializeField]
    HexCell[] neighbors;



    public HexCoords hexCoords;

    public TextMeshProUGUI hexLabel_Text;

  


    public GameObject HexObject;


    [Header("Colors")]
    public Color colorActive;
    public Color colorHover = Color.white;

    public Color colorRangeMin;
    public Color colorRangeMax;


    private Renderer hexRenderer;
    private MaterialPropertyBlock matPropBlock;


    /////////////////////////////////////////////////////////////////

    public HexDirection GetOppositeDirection(HexDirection direction)
    {
        int directionValue = (int)direction;

        if (directionValue < 3)
        {
            return direction + 3;
        }
        else
        {
            return direction - 3;
        }
    }

    /////////////////////////////////////////////////////////////////

    private void Awake()
    {
        matPropBlock = new MaterialPropertyBlock();
        hexRenderer = HexObject.GetComponent<Renderer>();
    }

    /////////////////////////////////////////////////////////////////

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)GetOppositeDirection(direction)] = this;
    }

    /////////////////////////////////////////////////////////////////

    public void SetLabel(int x, int z)
    {
        hexCoords = HexCoords.GenerateCoords(x, z);
        string hexID = (x.ToString() + "/" + z.ToString());

        hexLabel_Text.text = hexID;
        gameObject.name = hexID;
    }

    /////////////////////////////////////////////////////////////////

    public void TouchCell()
    {
        Debug.Log("Touched Hex Cell: " + hexCoords.X + " - " + hexCoords.Z);
    }

    public void ClickCell()
    {
        Debug.Log("Click Hex Cell: " + hexCoords.X + " - " + hexCoords.Z);


        UpdateCellColor(colorHover);
        
    }

    /////////////////////////////////////////////////////////////////

    public void GenerateCellColor()
    {
        float value = Random.Range(0, 1f);
        colorActive = Color.Lerp(colorRangeMin, colorRangeMax, value);
    }

    public void UpdateCellColor(Color newColor)
    {
        // Get the current value of the material properties in the renderer.
        hexRenderer.GetPropertyBlock(matPropBlock);
        // Assign our new value.
        matPropBlock.SetColor("_Color", newColor);
        // Apply the edited values to the renderer.
        hexRenderer.SetPropertyBlock(matPropBlock);
    }

    /////////////////////////////////////////////////////////////////
}




