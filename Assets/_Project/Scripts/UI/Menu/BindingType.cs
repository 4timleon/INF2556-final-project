using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBinding : MonoBehaviour {

    [SerializeField] private PlayerInputHandler.Binding binding;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;

    public PlayerInputHandler.Binding Binding => binding;
    public Button Button => button;
    public TextMeshProUGUI ButtonText => buttonText;


}
