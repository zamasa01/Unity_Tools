using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class PS_ChangeValue : MonoBehaviour
{
    public float timeDelay = 0;
    public bool timeDelay_Including_unActive = false;
    public bool timeDelay_Including_unEmission = false;

    public int OrderInLayer = 0;
    public bool OrderInLayer_Including_unActive = false;
    public bool OrderInLayer_Including_unEmission = false;

    public int OrderInLayer_SyncGreaterEqual = 1;
    public bool OrderInLayer_SyncGreaterEqual_unActive = false;
    public bool OrderInLayer_SyncGreaterEqual_unEmission = false;

    public float scaleStartSize = 100;
    public bool scaleStartSize_Including_unActive = false;
    public bool scaleStartSize_Including_unEmission = false;

    public void ScaleStartSize()
    {
        //Take all PS in this Object and Childs
        ParticleSystem[] childs = GetComponentsInChildren<ParticleSystem>(scaleStartSize_Including_unActive);
        if (childs.Length <= 0) { Debug.LogError("Can't find any ParticleSystem Component, please check again!!!"); return; }

        //Filter ignore unRenderer
        if (!scaleStartSize_Including_unEmission)
        {
            childs = childs.Where(childs => childs.emission.enabled == true).ToArray();
            if (childs.Length <= 0) { Debug.LogError("No ParticleSystem Component with spawn particle found, please check again!!!"); return; }
        }

        //Execute
        if (EditorUtility.DisplayDialog(
                "Confirmation",
                $"Do you want to Scale {scaleStartSize}% to 'Start Size' of {(scaleStartSize_Including_unActive ? "ALL CHILD" : "ACTIVE CHILD ONLY")} and {(scaleStartSize_Including_unEmission ? "INCLUDE UNEMISSION" : "NOT INCLUDE UNEMISSIONER")}?\n" +
                $"**The default behavior of this function will not include editing ParticleSystems with StartScale of type 'Curve' and 'Random Curve'**",
                "Yes Baby",
                "Damn No")
           )
        {
            childs
                .ToList()
                .ForEach(child =>
                {
                    var parMain = child.main;
                    if (parMain.startSize.mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        parMain.startSize = new ParticleSystem.MinMaxCurve(
                            (float)((decimal)parMain.startSize.constantMin * ((decimal)scaleStartSize / (decimal)100)),
                            (float)((decimal)parMain.startSize.constantMax * ((decimal)scaleStartSize / (decimal)100)));
                    }
                    else if (parMain.startSize.mode == ParticleSystemCurveMode.Constant)
                    { parMain.startSize = (float)((decimal)parMain.startSize.constant * ((decimal)scaleStartSize / (decimal)100)); }
                });
        }
    }

    public void PlusTimeDelay()
    {
        //Take all PS in this Object and Childs
        ParticleSystem[] childs = GetComponentsInChildren<ParticleSystem>(timeDelay_Including_unActive);
        if (childs.Length <= 0) { Debug.LogError("Can't find any ParticleSystem Component, please check again!!!"); return; }

        //Filter ignore unRenderer
        if (!timeDelay_Including_unEmission)
        {
            childs = childs.Where(childs => childs.emission.enabled == true).ToArray();
            if (childs.Length <= 0) { Debug.LogError("No ParticleSystem Component with spawn particle found, please check again!!!"); return; }
        }

        //Execute
        if (EditorUtility.DisplayDialog(
                "Confirmation",
                $"Do you want to add {timeDelay}s to DelayTime of {(timeDelay_Including_unActive ? "ALL CHILD" : "ACTIVE CHILD ONLY")} and {(timeDelay_Including_unEmission ? "INCLUDE UNEMISSION" : "NOT INCLUDE UNEMISSIONER")}?",
                "Yes Baby",
                "Damn No")
           )
        {
            childs
                .ToList()
                .ForEach(child =>
                {
                    var parMain = child.main;
                    if (parMain.startDelay.mode == ParticleSystemCurveMode.TwoConstants)
                    { 
                        parMain.startDelay = new ParticleSystem.MinMaxCurve(
                            (float)((decimal)parMain.startDelay.constantMin + ((decimal)timeDelay * (decimal)parMain.simulationSpeed)),
                            (float)((decimal)parMain.startDelay.constantMax + ((decimal)timeDelay * (decimal)parMain.simulationSpeed))); 
                    }
                    else if (parMain.startDelay.mode == ParticleSystemCurveMode.Constant)
                    { parMain.startDelay = (float)((decimal)parMain.startDelay.constant + ((decimal)timeDelay * (decimal)parMain.simulationSpeed)); }
                });
        }
    }

    public void PlusOrderInLayer()
    {
        //Take all PS in this Object and Childs
        ParticleSystemRenderer[] childs = GetComponentsInChildren<ParticleSystemRenderer>(OrderInLayer_Including_unActive);
        if (childs.Length <= 0) { Debug.LogError("Can't find any ParticleSystem Component, please check again!!!"); return; }

        //Filter ignore unRenderer
        if (!OrderInLayer_Including_unEmission)
        {
            childs = childs.Where(childs => childs.GetComponent<ParticleSystem>().emission.enabled == true).ToArray();
            if (childs.Length <= 0) { Debug.LogError("No ParticleSystem Component with spawn particle found, please check again!!!"); return; }
        }

        //Execute
        if (EditorUtility.DisplayDialog(
                "Confirmation",
                $"Do you want to add {OrderInLayer} to Order In Layer of {(OrderInLayer_Including_unActive ? "ALL CHILD" : "ACTIVE CHILD ONLY")} and {(OrderInLayer_Including_unEmission ? "INCLUDE UNEMISSION" : "NOT INCLUDE UNEMISSIONER")}?",
                "Yes Baby",
                "Damn No")
           )
        {
            childs.ToList().ForEach(child => child.sortingOrder += OrderInLayer);
        }
    }

    public void disableLoop()
    {
        ParticleSystem[] childs = GetComponentsInChildren<ParticleSystem>(true);
        if (childs.Length <= 0) { Debug.LogError("Can't find any ParticleSystem Component, please check again!!!"); return; }

        if (
            EditorUtility.DisplayDialog(
                "Confirmation",
                $"Do you want to dissable Loop Option of All Child (Including unEmission) ?",
                "Yes Baby",
                "Damn No")
           )
        {
            childs.ToList().ForEach(child => child.loop = false);
        }
    }

    public void OffsetDelayTimeBeginTo_0_All()
    {
        //Take all PS in this Object and Childs
        ParticleSystem[] childs = GetComponentsInChildren<ParticleSystem>(true);
        if (childs.Length <= 0) { Debug.LogError("Can't find any ParticleSystem Component, please check again!!!"); return; }

        //Filter ignore unRenderer
        childs = childs.Where(childs => childs.emission.enabled == true).ToArray();
        if (childs.Length <= 0) { Debug.LogError("No ParticleSystem Component with spawn particle found, please check again!!!"); return; }

        //Find min Delay Time
        float minDelayTime = 100000;
        childs
            .ToList()
            .ForEach(child => minDelayTime = child.main.startDelayMultiplier < minDelayTime ? child.main.startDelayMultiplier : minDelayTime);

        //Calculate
        float numNeedSubtract = minDelayTime > 0 ? minDelayTime : 0;

        //Execute
        if (
            EditorUtility.DisplayDialog(
                "Confirmation",
                $"Do you want to Offset Delay Time Begin To 0 _ All Child _ Not Including unEmission?\n" +
                $"Num need subtract = {numNeedSubtract}",
                "Yes Baby",
                "Damn No")
           )
        {
            childs
                .ToList()
                .ForEach(child => 
                {
                    var parMain = child.main;
                    if (parMain.startDelay.mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        parMain.startDelay = new ParticleSystem.MinMaxCurve(
                            (float)((decimal)parMain.startDelay.constantMin - ((decimal)numNeedSubtract * (decimal)parMain.simulationSpeed)),
                            (float)((decimal)parMain.startDelay.constantMax - ((decimal)numNeedSubtract * (decimal)parMain.simulationSpeed)));
                    }
                    else if (parMain.startDelay.mode == ParticleSystemCurveMode.Constant)
                    { parMain.startDelay = (float)((decimal)parMain.startDelay.constant - ((decimal)numNeedSubtract * (decimal)parMain.simulationSpeed)); }
                });
        }
    }

    public void SyncOrderInLayer_GreaterEqual()
    {

        //Take all PS in this Object and Childs
        ParticleSystemRenderer[] childs = GetComponentsInChildren<ParticleSystemRenderer>(OrderInLayer_SyncGreaterEqual_unActive);
        if (childs.Length <= 0) { Debug.LogError("Can't find any ParticleSystem Component, please check again!!!"); return; }

        //Filter ignore unEmission
        if (!OrderInLayer_SyncGreaterEqual_unEmission)
        {
            childs = childs.Where(childs => childs.GetComponent<ParticleSystem>().emission.enabled == true).ToArray();
            if (childs.Length <= 0) { Debug.LogError("No ParticleSystem Component with spawn particle found, please check again!!!"); return; }
        }

        //Find min
        int minOIL = childs[0].sortingOrder;
        childs.ToList().ForEach(child => minOIL = child.sortingOrder < minOIL ? child.sortingOrder : minOIL);

        //Calculate
        int numNeedPlus = minOIL < OrderInLayer_SyncGreaterEqual ? OrderInLayer_SyncGreaterEqual - minOIL : 0;
        
        //Confirm with Editer
        if (
            EditorUtility.DisplayDialog(
                "Confirmation",
                $"Do you want to Sync Order In Layer GreaterEqual {OrderInLayer_SyncGreaterEqual} of {(OrderInLayer_SyncGreaterEqual_unActive ? "ALL CHILD" : "ACTIVE CHILD ONLY")} and {(OrderInLayer_SyncGreaterEqual_unEmission ? "INCLUDE UNEMISSION" : "NOT INCLUDE UNEMISSIONER")}?\n" +
                $"Min OrderInLayer = {minOIL}\n" +
                $"Num need add = {numNeedPlus}",
                "Yes Baby",
                "Damn No")
           )
        {
            childs.ToList().ForEach(child => child.sortingOrder += numNeedPlus);
        }
    }
}

[CustomEditor(typeof(PS_ChangeValue))]
[CanEditMultipleObjects]
public class PS_ChangeValue_Inspector : Editor
{
    PS_ChangeValue mainScript;

    //Attribute size on Excel Zone
    float Excel_Width_LabelField = 130;
    float Excel_Width_ToggleField = 80;
    float Excel_Width_FloatField = 60;
    float Excel_Width_ButtonField = 60;
    float Excel_Height_Label = 40;

    //Attribute size on Normal Zone
    float Normal_Width_LabelField = 310;
    float Normal_Width_ButtonField = 100;

    public override void OnInspectorGUI()
    {
        mainScript = (PS_ChangeValue)serializedObject.targetObject;

        //Label
        EditorGUILayout.BeginHorizontal();
        GUILayout g = new GUILayout();
        EditorGUILayout.LabelField("Function Name", GUILayout.Width(Excel_Width_LabelField), GUILayout.Height(Excel_Height_Label));
        EditorGUILayout.LabelField("Include\nunActive", GUILayout.Width(Excel_Width_ToggleField), GUILayout.Height(Excel_Height_Label));
        EditorGUILayout.LabelField("Include\nunEmission", GUILayout.Width(Excel_Width_ToggleField), GUILayout.Height(Excel_Height_Label));
        EditorGUILayout.LabelField("Value", GUILayout.Width(Excel_Width_FloatField), GUILayout.Height(Excel_Height_Label));
        EditorGUILayout.EndHorizontal();
        drawLine(Color.white);

        //DelayTime Execute
        EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
        EditorGUILayout.LabelField("1. Add Delay Time", GUILayout.Width(Excel_Width_LabelField));
        mainScript.timeDelay_Including_unActive = EditorGUILayout.Toggle(mainScript.timeDelay_Including_unActive, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.timeDelay_Including_unEmission = EditorGUILayout.Toggle(mainScript.timeDelay_Including_unEmission, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.timeDelay = EditorGUILayout.FloatField(mainScript.timeDelay, GUILayout.Width(Excel_Width_FloatField));
        if (GUILayout.Button("Execute", GUILayout.Width(Excel_Width_ButtonField))) { mainScript.PlusTimeDelay(); }
        EditorGUILayout.EndHorizontal();

        //Order In Layer Execute
        EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
        EditorGUILayout.LabelField("2. Add Order In Layer", GUILayout.Width(Excel_Width_LabelField));
        mainScript.OrderInLayer_Including_unActive = EditorGUILayout.Toggle(mainScript.OrderInLayer_Including_unActive, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.OrderInLayer_Including_unEmission = EditorGUILayout.Toggle(mainScript.OrderInLayer_Including_unEmission, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.OrderInLayer = EditorGUILayout.IntField(mainScript.OrderInLayer, GUILayout.Width(Excel_Width_FloatField));
        if (GUILayout.Button("Execute", GUILayout.Width(Excel_Width_ButtonField))) { mainScript.PlusOrderInLayer(); }
        EditorGUILayout.EndHorizontal();

        //Start Size Execute
        EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
        EditorGUILayout.LabelField("3. Scale Start Size (%)", GUILayout.Width(Excel_Width_LabelField));
        mainScript.scaleStartSize_Including_unActive = EditorGUILayout.Toggle(mainScript.scaleStartSize_Including_unActive, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.scaleStartSize_Including_unEmission = EditorGUILayout.Toggle(mainScript.scaleStartSize_Including_unEmission, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.scaleStartSize = EditorGUILayout.FloatField(mainScript.scaleStartSize, GUILayout.Width(Excel_Width_FloatField));
        if (GUILayout.Button("Execute", GUILayout.Width(Excel_Width_ButtonField))) { mainScript.ScaleStartSize(); }
        EditorGUILayout.EndHorizontal();

        //Sync Order In Layer Greate Than Input Number
        EditorGUILayout.BeginHorizontal(GUILayout.Height(50));
        EditorGUILayout.LabelField("4. Order In Layer\nGreaterEqual", GUILayout.Width(Excel_Width_LabelField), GUILayout.Height(35));
        mainScript.OrderInLayer_SyncGreaterEqual_unActive = EditorGUILayout.Toggle(mainScript.OrderInLayer_SyncGreaterEqual_unActive, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.OrderInLayer_SyncGreaterEqual_unEmission = EditorGUILayout.Toggle(mainScript.OrderInLayer_SyncGreaterEqual_unEmission, GUILayout.Width(Excel_Width_ToggleField));
        mainScript.OrderInLayer_SyncGreaterEqual = EditorGUILayout.IntField(mainScript.OrderInLayer_SyncGreaterEqual, GUILayout.Width(Excel_Width_FloatField));
        if (GUILayout.Button("Execute", GUILayout.Width(Excel_Width_ButtonField))) { mainScript.SyncOrderInLayer_GreaterEqual(); }
        EditorGUILayout.EndHorizontal();
        drawLine(Color.white);

        //Function Dissable All Loop Option
        EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
        EditorGUILayout.LabelField("5. Dissable Loop Option of All Child", GUILayout.Width(Normal_Width_LabelField));
        if (GUILayout.Button("Execute", GUILayout.Width(Normal_Width_ButtonField))) { mainScript.disableLoop(); }
        EditorGUILayout.EndHorizontal();

        //Function Offset DelayTime Begin to 0s
        EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
        EditorGUILayout.LabelField("6. Offset DelayTime Begin from 0s", GUILayout.Width(Normal_Width_LabelField));
        if (GUILayout.Button("Execute", GUILayout.Width(Normal_Width_ButtonField))) { mainScript.OffsetDelayTimeBeginTo_0_All(); }
        EditorGUILayout.EndHorizontal();

        //Old Inspector
        //EditorGUILayout.Space(15);
        //drawLine(Color.white);
        //label("OLD", 24, 30, Color.white);
        //base.OnInspectorGUI();
    }

    void drawLine(Color color)
    {
        EditorGUILayout.BeginVertical(GUILayout.Height(1));
        Rect rect = EditorGUILayout.GetControlRect();
        rect.height = 1;
        EditorGUI.DrawRect(rect, color);
        EditorGUILayout.EndVertical();
    }
}