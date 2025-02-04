using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabelsManager : MonoBehaviour
{
    public SpawnSongRoad song;
    [SerializeField] RectTransform labelsPanel;
    [SerializeField] Text inputTitleText;
    [SerializeField] Text createNewButtonText;
    [SerializeField] Button buttonPrefab;
    [SerializeField] InputField labelInput;

    List<Button> labelButtons;
    List<int> labelFretInd;
    public void SpawnLabels()
    {
        if (labelButtons != null)
        {
            for (int i = labelButtons.Count - 1; i >= 0; i--)
            {
                Destroy(labelButtons[i].gameObject);
            }

            labelButtons.Clear();
            labelFretInd.Clear();
        } else
        {
            labelButtons = new List<Button>();
            labelFretInd = new List<int>();
        }

        numButtons = 1;

        for (int i = 0; i < song.frets.Count; i++)
        {
            TextMesh label = song.frets[i].trans.GetComponentInChildren<TextMesh>();
            label.text = i.ToString();
            if (song.frets[i].label.Length > 0)
            {
                // fret labels
                label.text += ": " + song.frets[i].label;

                // label list
                numButtons++;

                Button newButton = Instantiate(buttonPrefab, labelsPanel);
                newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-10, -(numButtons - 1) * 105 - 15);
                newButton.GetComponentInChildren<Text>().text = i.ToString() + ": " + song.frets[i].label;
                
                newButton.GetComponentsInChildren<Button>()[1].onClick.AddListener(() => DeleteLabel(newButton));
                newButton.onClick.AddListener(() => MoveToLabel(newButton));
                newButton.name = i.ToString();

                labelButtons.Add(newButton);
                labelFretInd.Add(i);
            }
        }
        
        labelsPanel.sizeDelta = new Vector2(0, numButtons * 105 + 15);
    }

    int numButtons = 1;
    public void CreateNewLabel()
    {
        numButtons++;
        labelsPanel.sizeDelta = new Vector2(0, numButtons * 105 + 15);

        song.frets[song.curFretInd].label = labelInput.text;
        song.frets[song.curFretInd].trans.GetComponentInChildren<TextMesh>().text = song.curFretInd.ToString() + ": " + labelInput.text;

        Button newButton = Instantiate(buttonPrefab, labelsPanel);
        newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-10, -(numButtons - 1) * 105 - 15);
        newButton.GetComponentInChildren<Text>().text = song.curFretInd.ToString() + ": " + labelInput.text;
        newButton.GetComponentsInChildren<Button>()[1].onClick.AddListener(() => DeleteLabel(newButton));
        newButton.onClick.AddListener(() => MoveToLabel(newButton));
        newButton.name = song.curFretInd.ToString();

        labelButtons.Add(newButton);
        labelFretInd.Add(song.curFretInd);

        labelInput.text = "";
    }

    private void Update()
    {
        createNewButtonText.text = "New label at: " + song.curFretInd;
        inputTitleText.text = "Create label at: " + song.curFretInd;
    }

    public void FreezeControls(bool val)
    {
        song.inMenu = val;
    }

    public void DeleteLabel(Button button)
    {
        int fretInd = System.Convert.ToInt32(button.name);
        
        for (int i = 0; i < labelButtons.Count; i++)
        {
            if (labelFretInd[i] == fretInd)
            {
                song.frets[labelFretInd[i]].label = "";
                break;
            }
        }

        SpawnLabels();
    }

    public void MoveToLabel(Button button)
    {
        int fretInd = System.Convert.ToInt32(button.name);

        song.curFretInd = fretInd;
        song.movingTo = true;
        StartCoroutine(song.MoveToFret());
    }
}
