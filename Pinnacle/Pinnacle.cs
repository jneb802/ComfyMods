﻿using BepInEx;

using HarmonyLib;

using System.Collections;
using System.Linq;
using System.Reflection;

using UnityEngine;

using static Pinnacle.PluginConfig;

namespace Pinnacle {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Pinnacle : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.pinnacle";
    public const string PluginName = "Pinnacle";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static PinEditPanel PinEditPanel { get; private set; }

    public static void TogglePinEditPanel(Minimap.PinData pin) {
      if (!PinEditPanel?.Panel) {
        PinEditPanel = new(Minimap.m_instance.m_largeRoot.transform);
        PinEditPanel.Panel.RectTransform()
            .SetAnchorMin(new(0.5f, 0f))
            .SetAnchorMax(new(0.5f, 0f))
            .SetPivot(new(0.5f, 0f))
            .SetPosition(new(0f, 25f))
            .SetSizeDelta(new(200f, 200f));
      }

      if (pin == null) {
        PinEditPanel.Panel.SetActive(false);
      } else {
        CenterMapOnPinPosition(pin.m_pos);

        PinEditPanel.SetTargetPin(pin);
        PinEditPanel.Panel.SetActive(true);
      }
    }

    public static PinListPanel PinListPanel { get; private set; }

    public static void TogglePinListPanel() {
      if (!PinListPanel?.Panel) {
        PinListPanel = new(Minimap.m_instance.m_largeRoot.transform);
        PinListPanel.Panel.RectTransform()
            .SetAnchorMin(new(0f, 0.5f))
            .SetAnchorMax(new(0f, 0.5f))
            .SetPivot(new(0f, 0.5f))
            .SetPosition(new(25f, 0f))
            .SetSizeDelta(new(400f, 400f));
      }

      if (PinListPanel.Panel.activeSelf) {
        PinListPanel.Panel.SetActive(false);
      } else {
        PinListPanel.Panel.SetActive(true);
        //TODO: remove limit from 30, just for testing right now.
        PinListPanel.SetTargetPins(Minimap.m_instance.m_pins.Take(30).OrderBy(p => p.m_name).ToList());
      }
    }

    static Coroutine _centerMapCoroutine;

    public static void CenterMapOnPinPosition(Vector3 targetPosition) {
      if (_centerMapCoroutine != null) {
        Minimap.m_instance.StopCoroutine(_centerMapCoroutine);
      }

      _centerMapCoroutine =
          Minimap.m_instance.StartCoroutine(
              CenterMapCoroutine(
                  targetPosition - Player.m_localPlayer.transform.position, CenterMapLerpDuration.Value));
    }

    static IEnumerator CenterMapCoroutine(Vector3 targetPosition, float lerpDuration) {
      float timeElapsed = 0f;
      Vector3 startPosition = Minimap.m_instance.m_mapOffset;

      while (timeElapsed < lerpDuration) {
        float t = timeElapsed / lerpDuration;
        t = t * t * (3f - (2f * t));

        Minimap.m_instance.m_mapOffset = Vector3.Lerp(startPosition, targetPosition, t);
        timeElapsed += Time.deltaTime;

        yield return null;
      }

      Minimap.m_instance.m_mapOffset = targetPosition;
    }
  }
}