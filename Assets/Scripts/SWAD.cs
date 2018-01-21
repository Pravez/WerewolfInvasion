using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SWAD : MonoBehaviour
{
    public static Ui GameUi;

	public Material Land;
	private const int Size = 600;
	public Texture2D Tx;
	private Color[] _pixels;

	public float S0 = 0.9f, A0 = 0, W0 = 0.1f;
	public float S, A, W;

	//Coeff armed peasants
	public float Alpha = 0.02f;
	//Coeff werewolves apparition
	public float Beta = 0.04f;
	//Coeff peasants killing werewolves
	public float Gamma = 0.05f;
	//Coeff armed becoming werewolves
	public float Delta = 0.05f;
	//Coeff werewolves dying
	public float Psi = 0;//0.007f;
	//Coeff peasants reproducing
	public float Phi = 0.005f;//0.002f;
	
	//Delta of update time
	public float DeltaTime = 1f;
	
	private int _peasants = Size;
	private int _werewolves = 0;
	private int _armedPeasants = 0;

	private int[] _graphicsPeasants;
	private int[] _graphicsWerevolves;
	private int[] _graphicsArmed;
	private int _currentState;
	
	void Start () {
		Tx= new Texture2D(Size, Size);
		_pixels = new Color[Size * Size];
		
		_graphicsPeasants = new int[Size];
		_graphicsWerevolves = new int[Size];
		_graphicsArmed = new int[Size];
		_currentState = 0;
		
		ResetValues();
		//UpdateGraphics(_peasants, _werewolves, _armedPeasants);
	}
	
	void Update ()
	{
		//ResetValues();
		//for (int i = 0; i < Size; ++i)
		//{
			NextStep();
		//}

		//DrawGraphics();
		
		//Land.SetTexture("_MainTex", Tx);
		//Tx.SetPixels(_pixels);
		//Tx.Apply();

	    UpdateUi();
	}
	
	
	private void DrawPoint(Vector2 p, Color c)
	{
		if (p.x < Size && p.x >= 0 && p.y < Size && p.y >= 0)
			_pixels[(int) p.x * Size + (int) p.y] = c;
	}

	private void UpdateGraphics(int peasants, int werewolves, int armed)
	{
		_graphicsPeasants[_currentState] = peasants;
		_graphicsWerevolves[_currentState] = werewolves;
		_graphicsArmed[_currentState] = armed;

		_currentState++;
	}

	private void DrawGraphics()
	{
		for (int i = 0; i < Size * Size; ++i)
			_pixels[i] = Color.white;

		for (int i = 0; i < _currentState; ++i)
		{
			DrawPoint(new Vector2(_graphicsPeasants[i], i), Color.blue);
			DrawPoint(new Vector2(_graphicsWerevolves[i], i), Color.red);
			DrawPoint(new Vector2(_graphicsArmed[i], i), Color.black);
		}
	}

	private void ResetValues()
	{
		S = S0;
		W = W0;
		A = A0;

	    _currentState = 0;
	}

	public void NextStep()
	{
		
		float nS = ( - Alpha * S - Beta * S * W + Phi * ((S + A)*(S+A))) * Time.deltaTime;
		float nA = (Alpha * S - Delta * A * W) * Time.deltaTime;
		float dW = (Beta * S * W + Delta * A * W - Gamma * A * W - Psi * W) * Time.deltaTime;

		S = ((S + nS) < 0 ? 0 : S + nS);
		A = A + nA;
		W = W + dW;
			
		//UpdateGraphics((int) (S*Size), (int) (W*Size), (int) (A*Size));
	}

	public int GetPeasants()
	{
		return (int) (S * Size);
	}

	public int GetArmedPeasants()
	{
		return (int) (A * Size);
	}

	public int GetWerewolves()
	{
		return (int) (W * Size);
	}

	public int GetDeads()
	{
		return (int) (1f - (S + W + A));
	}

    private void UpdateUi()
    {
        if (GameUi != null)
        {
            GameUi.SetPeasants(GetPeasants());
            GameUi.SetWerewolves(GetWerewolves());
            GameUi.SetArmed(GetArmedPeasants());
        }
    }

    public void AddForgeToChangePeasantsToArmed()
    {
        // Peasants grow faster and are better at killing werewolves
        Alpha += 0.005f;
        Gamma += 0.005f;
    }

	public void KillWerewolf()
	{
		W -= 0.01f;
	}
}
