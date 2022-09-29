using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace InteractionTK.Menus
{
    public class MTKKeypadWindow : MTKWindow
    {
        public MTKButton key0;
        public MTKButton key1;
        public MTKButton key2;
        public MTKButton key3;
        public MTKButton key4;
        public MTKButton key5;
        public MTKButton key6;
        public MTKButton key7;
        public MTKButton key8;
        public MTKButton key9;
        public MTKButton keyDot;
        public MTKButton keyColon;
        public MTKButton keyDelete;
        public MTKButton keySubmit;
        public MTKButton keyClear;

        public TextMeshProUGUI value;
        public TextMeshProUGUI message;

        public UnityEvent<string> OnSubmit;
        public UnityEvent OnClose;

        public override void Start()
        {
            base.Start();
            
            key0.OnClick.AddListener(() => value.text += "0");
            key1.OnClick.AddListener(() => value.text += "1");
            key2.OnClick.AddListener(() => value.text += "2");
            key3.OnClick.AddListener(() => value.text += "3");
            key4.OnClick.AddListener(() => value.text += "4");
            key5.OnClick.AddListener(() => value.text += "5");
            key6.OnClick.AddListener(() => value.text += "6");
            key7.OnClick.AddListener(() => value.text += "7");
            key8.OnClick.AddListener(() => value.text += "8");
            key9.OnClick.AddListener(() => value.text += "9");
            keyDot.OnClick.AddListener(() => value.text += ".");
            keyColon.OnClick.AddListener(() => value.text += ":");
            keyDelete.OnClick.AddListener(() => value.text = value.text.Remove(value.text.Length - 1));
            keySubmit.OnClick.AddListener(() => OnSubmit?.Invoke(value.text));
            keyClear.OnClick.AddListener(() => value.text = string.Empty);
        }

        public override void CloseWindow()
        {
            OnClose?.Invoke();
        }
    }
}