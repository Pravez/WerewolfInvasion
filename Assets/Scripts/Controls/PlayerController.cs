using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public const string Name = "Player";
    public static GenerateLand land;
    public static float Gravity = 9.8f;
    public static float Speed = 10f; // units per second
    public static float RotationSpeed = 8f;
    public static SWAD _simulation;
    private CharacterController _player;

    private float _moveLr;
    private float _moveFb;
    private float _verticalSpeed;

    private bool isPlacingForge;
    private GameObject forge;

    private const int MaxPossibleForges = 5;
    private int _placedForges = 0;

    public Ui Ui;
    private ParticleSystem _particleSystem;

    private int _currentResources = 100;
    public const int MinUnitsToBuildForge = 100;

    void Start()
    {
        _verticalSpeed = 0f;

        _player = GetComponent<CharacterController>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _particleSystem.Stop();

        Ui.SetResources(_currentResources);
    }

    void Update()
    {
        float speed = Speed;

        if (Input.GetKey(KeyCode.LeftControl))
            speed *= 4;

        transform.Rotate(0, Input.GetAxis("Horizontal") * RotationSpeed, 0);
        _moveFb = Input.GetAxis("Vertical") * speed;

        //Applying gravity
        _verticalSpeed -= Gravity * Time.deltaTime;
        Vector3 movement = new Vector3(_moveLr, _verticalSpeed, _moveFb);


        movement = transform.rotation * movement;

        if (Input.GetKey(KeyCode.Space))
        {
            _verticalSpeed = 10f;
        }

        //Calculate rotation in order to rotate with mouse
        /*float yRot = Input.GetAxisRaw("Mouse X");
        Vector3 rotation = new Vector3(0f, yRot, 0f) * 3f;
        transform.Rotate(rotation);*/

        _player.Move(movement * Time.deltaTime);

        HandleForge();
        HandleFire();
    }

    void HandleFire()
    {
        if (!isPlacingForge && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(0)))
        {
            _particleSystem.Play();
            land.KillAroundPosition(transform.position);
        }
        else if (isPlacingForge || (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetMouseButtonUp(0)))
        {
            _particleSystem.Stop();
        }

        // For debug
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameManager.Instance.GenerateLabyrinth(new Vector3(transform.position.x, land.GetHeightOnWorldPosition(transform.position.x, transform.position.z + 10), transform.position.z + 10), 
                new Vector3Int(5, 5, 1));
        }
    }

    void HandleForge()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_placedForges < MaxPossibleForges)
            {
                isPlacingForge = !isPlacingForge;
                if (isPlacingForge)
                {
                    Ui.SetForgeText("Place your forge");
                    Ui.ChangeForgeTextColor(Color.white);

                    forge = Instantiate(GameManager.Instance.AssetManager.Forge, Vector3.zero,
                        Quaternion.identity);
                    forge.transform.SetParent(transform);
                    forge.transform.localPosition = new Vector3(3, 0, 10);
                    forge.transform.rotation = Quaternion.identity;
                    forge.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                }
                else
                {
                    Ui.SetForgeText("");
                    Destroy(forge);
                }
            }
            else
            {
                // Cannot place forges anymore
                Ui.SetForgeText("Cannot place any more forges !");
                Ui.ChangeForgeTextColor(Color.red);
            }
        }

        if (Input.GetKeyUp(KeyCode.F) && !isPlacingForge)
        {
            Ui.SetForgeText("");
        }

        if (isPlacingForge && land != null)
        {
            forge.transform.position = new Vector3(forge.transform.position.x,
                land.GetHeightOnWorldPosition(forge.transform.position.x, forge.transform.position.z),
                forge.transform.position.z);
            if (Input.GetMouseButtonDown(0))
            {
                if (_currentResources >= MinUnitsToBuildForge)
                {
                    if (land.AddForgeOnPosition(forge.transform.position.x, forge.transform.position.z, forge))
                    {
                        isPlacingForge = false;
                        forge = null;
                        _placedForges++;
                        Ui.SetPlacedForges(_placedForges);
                        Ui.SetForgeText("");
                        _currentResources -= MinUnitsToBuildForge;
                        Ui.SetResources(_currentResources);
                    }
                    else
                    {
                        // could not place the forge
                        Ui.SetForgeText("Only one forge per chunk !");
                        Ui.ChangeForgeTextColor(Color.red);
                    }
                }
                else
                {
                    // could not place the forge
                    Ui.SetForgeText("You need more resources ! (100 min)");
                    Ui.ChangeForgeTextColor(Color.red);
                }
            }
        }
    }

    public void GatherResources(int amount)
    {
        _currentResources += amount;
        Ui.SetResources(_currentResources);
    }
}