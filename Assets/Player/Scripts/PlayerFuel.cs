using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerFuel : MonoBehaviour 
{ 
    [Header("UI Settings")]
    [SerializeField] TextMeshProUGUI fuelText;
    
    [Header("Settings")]
    [Range(0f, 10f)] [Tooltip("Fuel per unit traveled forward")]
    public float fuelConsumption;
    [Range(0f, 100f)]
    public float startingFuel;
    
    private PlayerDeath _playerDeath;
    
    private float _lastFrameZPos;
    private float _fuel;

    void Awake()
    {
        _playerDeath = GetComponent<PlayerDeath>();
        _lastFrameZPos = transform.position.z;
        _fuel = startingFuel;
    }

    void Update()
    {
        UseFuel();
        if(_fuel <= 0f) _playerDeath.DieNoFuel();
    }

    private void UseFuel()
    {
        float usedFuel = (transform.position.z - _lastFrameZPos) * fuelConsumption;
        _fuel -= usedFuel;
        _fuel = Mathf.Clamp(_fuel, 0f, startingFuel);
        _lastFrameZPos = transform.position.z;
        
        DisplayFuel();
    }

    private void DisplayFuel()
    {
        fuelText.text = "Fuel:\n" + _fuel.ToString("F1");
    }
}
