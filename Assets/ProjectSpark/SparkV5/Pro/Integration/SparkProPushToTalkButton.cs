using UnityEngine;
using UnityEngine.EventSystems;
namespace ProjectSpark.SparkV5.Pro
{
    public sealed class SparkProPushToTalkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] SparkProVoiceInput microphone; int pointer=int.MinValue;
        public void Bind(SparkProVoiceInput input) => microphone = input;
        public void OnPointerDown(PointerEventData eventData){if(pointer!=int.MinValue||eventData.button!=PointerEventData.InputButton.Left||!microphone)return;pointer=eventData.pointerId;microphone.BeginPushToTalk();}
        public void OnPointerUp(PointerEventData eventData){if(eventData.pointerId==pointer)Release(false);}
        public void OnPointerExit(PointerEventData eventData){if(eventData.pointerId==pointer)Release(true);}
        void OnDisable(){Release(true);}
        void Release(bool cancel){if(pointer==int.MinValue)return;pointer=int.MinValue;if(microphone){if(cancel)microphone.Stop();else microphone.EndPushToTalk();}}
    }
}
