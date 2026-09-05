using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBinding : MonoBehaviour
{

    [SerializeField] private GeneralInputHandler.Binding binding;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;

    public GeneralInputHandler.Binding Binding => binding;
    public Button Button => button;
    public TextMeshProUGUI ButtonText => buttonText;


}
