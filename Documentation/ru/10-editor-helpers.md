# Editor Helpers

Хелперы отображаемых имён объектов Unity для кастомных редакторов:

| Метод | Возвращает |
|---|---|
| `GetScriptName()` | Отображаемое имя объекта — `ObjectNames.GetInspectorTitle`, если у типа есть `[AddComponentMenu]`, иначе «очеловеченное» имя типа |
| `GetScriptNameWithIndex()` | То же имя с числовым суффиксом, когда на GameObject несколько компонентов одного типа — например `"Audio Source (2)"` |

```csharp
using Aspid.FastTools.Editors;

[CustomEditor(typeof(MyBehaviour))]
public class MyBehaviourEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // "My Behaviour" — или "Custom Name", если присутствует [AddComponentMenu("Custom Name")]
        var name = target.GetScriptName();

        // "My Behaviour (2)" при наличии второго компонента того же типа
        var nameWithIndex = ((Component)target).GetScriptNameWithIndex();

        return new Label(name);
    }
}
```
