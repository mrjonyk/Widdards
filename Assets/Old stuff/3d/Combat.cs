using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Combat : NetworkBehaviour 
{
	public const int startingHealth = 100;                            // The amount of health the player starts the game with.
	[SyncVar(hook = "OnChangeHealth")]
	public int health;                                   // The current health the player has.
	public Slider healthSlider;                                 // Reference to the UI's health bar.

	bool isDead;                                                // Whether the player is dead.
	
	
	void Awake ()
	{
		// Set the initial health of the player.
		health = startingHealth;
	}
	

	public void TakeDamage(int amount)
	{
		if (!isServer) 
			return;
		health -= amount;

		if (health <= 0)
		{
			health = 0;
			Debug.Log("Dead!");
		}
	}

	void OnChangeHealth (int currentHealth)
	{
		healthSlider.value = currentHealth;
	}
}
