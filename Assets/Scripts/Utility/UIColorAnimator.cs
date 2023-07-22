using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIColorAnimator : MonoBehaviour
{
    public enum Type { Text, TMPro, Image }
    public enum Mode { Repeat, PingPong }

    public Type type;
    public Mode mode;
    public Color color1;
    public Color color2;
    public float speed = 1f;


    abstract class GraphicElement { public abstract void SetColor(Color c); }
    class TextElement<T> : GraphicElement where T : Graphic
    {
        T graphics;
        public TextElement(T element)
        {
            graphics = element;
        }
        public override void SetColor(Color c) { graphics.color = c; }
    }
    GraphicElement element;

    float time = 0;

    void OnEnable()
    {
        switch (type)
        {
            case Type.Text: element = new TextElement<Text>(GetComponent<Text>()); break;
            case Type.TMPro: element = new TextElement<TMP_Text>(GetComponent<TMP_Text>()); break;
            case Type.Image: element = new TextElement<Image>(GetComponent<Image>()); break;
        }
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        time += Time.deltaTime * speed;

        float t = time;
        switch (mode)
        {
            case Mode.PingPong: t = Mathf.PingPong(time, 1); break;
            case Mode.Repeat: t = Mathf.Repeat(time, 1); break;
        }

        Color c = Color.Lerp(color1, color2, t);
        element.SetColor(c);
    }

}
