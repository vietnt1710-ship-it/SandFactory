using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

namespace PolygonArsenal
{

public class PolygonSceneSelect : MonoBehaviour
{
	public bool GUIHide = false;
	public bool GUIHide2 = false;
	public bool GUIHide3 = false;
	public bool GUIHide4 = false;
	public bool GUIHide5 = false;
	
	//Combat Scenes
	
    public void CBLoadSceneMissiles()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyMissiles");	}
	public void CBLoadSceneBeams()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyBeams"); 		}
	public void CBLoadSceneBeams2()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyBeams2"); 	}
	public void CBLoadSceneAura()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyAura"); 		}
	public void CBLoadSceneAura2()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyAura2");	 	}
	public void CBLoadSceneAura3()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyAura3"); 		}
	public void CBLoadSceneAura4()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyAura4"); 		}
	public void CBLoadSceneBarrage()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyBarrage"); 	}
	public void CBLoadSceneBarrage2()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyBarrage2"); 	}
	public void CBLoadSceneChains()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyChains"); 	}
	public void CBLoadSceneChains2()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyChains2"); 	}
	public void CBLoadSceneCleave()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyCleave"); 	}
	public void CBLoadSceneCombat01()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyCombat01"); 	}
	public void CBLoadSceneCombat02()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyCombat02"); 	}
	public void CBLoadSceneCurses()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyCurses"); 	}
	public void CBLoadSceneDeath()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyDeath");	 	}
	public void CBLoadSceneEnchant()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyEnchant"); 	}
	public void CBLoadSceneExploMini()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyExploMini"); 	}
	public void CBLoadSceneGore()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyGore"); 		}
	public void CBLoadSceneHitscan()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyHitscan"); 	}
	public void CBLoadSceneNecromancy()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyNecromancy");	}
	public void CBLoadSceneNova()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyNova"); 		}
	public void CBLoadSceneOrbitalBeam()	{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyOrbitalBeam");}
	public void CBLoadSceneSpikes()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySpikes"); 	}
	public void CBLoadSceneSpikes2()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySpikes2"); 	}
	public void CBLoadSceneSpikes3()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySpikes3"); 	}
	public void CBLoadSceneSpikes4()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySpikes4"); 	}
	public void CBLoadSceneSurfaceDmg()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySurfaceDmg");	}
	public void CBLoadSceneSword()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySword"); 		}
	public void CBLoadSceneSwordTrail()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySwordTrail");	}
	
	//Environment Scenes
	
	public void ENVLoadSceneConfetti()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyConfetti"); 	}
	public void ENVLoadSceneEnvironment()	{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyEnvironment");}
	public void ENVLoadSceneFire()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyFire"); 		}
	public void ENVLoadSceneFire2()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyFire2"); 		}
	public void ENVLoadSceneFireflies()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyFireflies"); 	}
	public void ENVLoadSceneFireworks()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyFireworks"); 	}
	public void ENVLoadSceneLiquid()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyLiquid"); 	}
	public void ENVLoadSceneLiquid2()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyLiquid2"); 	}
	public void ENVLoadSceneRocks()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyRocks"); 		}
	public void ENVLoadSceneSparks()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySparks"); 	}
	public void ENVLoadSceneTornado()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyTornado"); 	}
	public void ENVLoadSceneWeather()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyWeather"); 	}
	
	//Interactive Scenes
	
	public void INTLoadSceneBeamUp()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyBeamUp"); 	}
	public void INTLoadSceneBlackHole()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyBlackHole"); 	}
	public void INTLoadSceneHeal()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyHeal"); 		}
	public void INTLoadSceneJets()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyJets"); 		}
	public void INTLoadSceneLoot()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyLoot"); 		}
	public void INTLoadScenePortal()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyPortal"); 	}
	public void INTLoadScenePortal2()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyPortal2"); 	}
	public void INTLoadScenePowerupIcon()	{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyPowerupIcon");}
	public void INTLoadSceneSpawn()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolySpawn"); 		}
	public void INTLoadSceneTrails()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyTrails"); 	}
	public void INTLoadSceneTreasure()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyTreasure"); 	}
	public void INTLoadSceneTreasure2()		{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyTreasure2"); 	}
	public void INTLoadSceneZones()			{ UnityEngine.SceneManagement.SceneManager.LoadScene("PolyZones"); 		}
	
	 void Update ()
	 {
 
     if(Input.GetKeyDown(KeyCode.L))
	 {
         GUIHide = !GUIHide;
     
         if (GUIHide)
		 {
             GameObject.Find("CanvasSceneSelectCom").GetComponent<Canvas> ().enabled = false;
         }
		 
		 else
		 {
             GameObject.Find("CanvasSceneSelectCom").GetComponent<Canvas> ().enabled = true;
         }
     }
	      if(Input.GetKeyDown(KeyCode.J))
	 {
         GUIHide2 = !GUIHide2;
     
         if (GUIHide2)
		 {
             GameObject.Find("CanvasMissiles").GetComponent<Canvas> ().enabled = false;
         }
		 else
		 {
             GameObject.Find("CanvasMissiles").GetComponent<Canvas> ().enabled = true;
         }
     }
		if(Input.GetKeyDown(KeyCode.H))
	 {
         GUIHide3 = !GUIHide3;
     
         if (GUIHide3)
		 {
             GameObject.Find("CanvasTips").GetComponent<Canvas> ().enabled = false;
         }
		 else
		 {
             GameObject.Find("CanvasTips").GetComponent<Canvas> ().enabled = true;
         }
     }
	 if(Input.GetKeyDown(KeyCode.M))
	 {
         GUIHide4 = !GUIHide4;
     
         if (GUIHide4)
		 {
             GameObject.Find("CanvasSceneSelectInt").GetComponent<Canvas> ().enabled = false;
         }
		 
		 else
		 {
             GameObject.Find("CanvasSceneSelectInt").GetComponent<Canvas> ().enabled = true;
         }
     }
	 if(Input.GetKeyDown(KeyCode.N))
	 {
         GUIHide5 = !GUIHide5;
     
         if (GUIHide5)
		 {
             GameObject.Find("CanvasSceneSelectEnv").GetComponent<Canvas> ().enabled = false;
         }
		 
		 else
		 {
             GameObject.Find("CanvasSceneSelectEnv").GetComponent<Canvas> ().enabled = true;
         }
     }
	}
}

}