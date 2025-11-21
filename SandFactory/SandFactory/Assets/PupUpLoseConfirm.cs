using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PupUpLoseConfirm : PopUp
{
    public Button btn_tryAgain;

    public override void MiniSub()
    {
        //UIManager.I.eventManager.Subscribe(EventManager.Event.close_lose_gameplay, base.Close);

        //btn_Home.onClick.AddListener(BackHomeEvent);
        //btn_TryAgain.onClick.AddListener(TryAgainEvent);

        btn_tryAgain.onClick.AddListener(ActionTryAgain);
    }

    public void ActionTryAgain()
    {
        Close();
        DOVirtual.DelayedCall(tweenDuration, () =>
        {
            SceneManager.LoadScene(0);
        });
    }

}
