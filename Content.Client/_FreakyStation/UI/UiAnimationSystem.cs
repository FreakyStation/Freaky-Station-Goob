// SPDX-FileCopyrightText: 2026 Freak
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Client.Animations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._FreakyStation.UI;

/// <summary>
/// Animated UI transitions using RobustToolbox's built-in Animation system.
/// <list type="bullet">
/// <item>Window entrance: fade-in from transparent (220ms)</item>
/// <item>Button hover: smooth modulate dim/brighten (120ms)</item>
/// <item>Click pop: quick dim-and-release (150ms)</item>
/// <item>Accent pulse: sine-wave modulate oscillation for highlighted elements</item>
/// </list>
/// </summary>
public sealed class UiAnimationSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private readonly HashSet<Control> _seenWindows = new();
    private readonly Dictionary<Control, float> _pulsingControls = new();
    private readonly List<Control> _pulseRemove = new();

    private const float EntranceDuration = 0.22f;
    private const float HoverDuration = 0.12f;
    private const float ClickDuration = 0.15f;
    private const float PulsePeriod = 1.6f;

    private const string HoverKey = "freaky-hover";
    private const string ClickKey = "freaky-click";
    private const string EntranceKey = "freaky-entrance";

    // Slight tint for hover state — subtle cool darken
    private static readonly Color HoverTint = new(0.88f, 0.88f, 0.88f, 1f);
    // Stronger dim for click press
    private static readonly Color PressTint = new(0.78f, 0.78f, 0.78f, 1f);

    private static readonly Animation EntranceAnim = new()
    {
        Length = TimeSpan.FromSeconds(EntranceDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, EntranceDuration),
                }
            },
        }
    };

    private static readonly Animation HoverInAnim = new()
    {
        Length = TimeSpan.FromSeconds(HoverDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(Color.White, 0f),
                    new AnimationTrackProperty.KeyFrame(HoverTint, HoverDuration),
                }
            },
        }
    };

    private static readonly Animation HoverOutAnim = new()
    {
        Length = TimeSpan.FromSeconds(HoverDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(HoverTint, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, HoverDuration),
                }
            },
        }
    };

    private static readonly Animation ClickPopAnim = new()
    {
        Length = TimeSpan.FromSeconds(ClickDuration),
        AnimationTracks =
        {
            new AnimationTrackControlProperty
            {
                Property = "Modulate",
                KeyFrames =
                {
                    new AnimationTrackProperty.KeyFrame(PressTint, 0f),
                    new AnimationTrackProperty.KeyFrame(Color.White, ClickDuration),
                }
            },
        }
    };

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        DetectNewWindows();
        UpdatePulse(frameTime);
    }

    private void DetectNewWindows()
    {
        foreach (var child in _uiManager.WindowRoot.Children)
        {
            if (child is not BaseWindow window)
                continue;

            if (!_seenWindows.Add(window))
                continue;

            // Entrance: fade-in from transparent
            window.Modulate = Color.White.WithAlpha(0f);
            window.PlayAnimation(EntranceAnim, EntranceKey);
        }

        _seenWindows.RemoveWhere(c => c.Disposed);
    }

    private void UpdatePulse(float frameTime)
    {
        foreach (var (control, phase) in _pulsingControls)
        {
            if (control.Disposed)
            {
                _pulseRemove.Add(control);
                continue;
            }

            var t = (phase + frameTime) % PulsePeriod;
            _pulsingControls[control] = t;

            // Sine-wave pulse: modulate oscillates 0.7 → 1.0 → 0.7
            var pulse = 0.85f + 0.15f * MathF.Sin(t * MathF.Tau / PulsePeriod);
            control.Modulate = new Color(pulse, pulse, pulse, 1f);
        }

        foreach (var control in _pulseRemove)
            _pulsingControls.Remove(control);
        _pulseRemove.Clear();
    }

    /// <summary>
    /// Smooth hover-in animation for any button-like control.
    /// Call on mouse enter.
    /// </summary>
    public static void AnimateHoverIn(Control control)
    {
        if (control.Disposed)
            return;
        control.StopAnimation(HoverKey);
        control.PlayAnimation(HoverInAnim, HoverKey);
    }

    /// <summary>
    /// Smooth hover-out animation for any button-like control.
    /// Call on mouse leave.
    /// </summary>
    public static void AnimateHoverOut(Control control)
    {
        if (control.Disposed)
            return;
        control.StopAnimation(HoverKey);
        control.PlayAnimation(HoverOutAnim, HoverKey);
    }

    /// <summary>
    /// Quick pop animation for button clicks — dim then release.
    /// </summary>
    public static void AnimateClickPop(Control control)
    {
        if (control.Disposed)
            return;
        control.StopAnimation(ClickKey);
        control.PlayAnimation(ClickPopAnim, ClickKey);
    }

    /// <summary>
    /// Add a subtle pulsing glow to a control (important buttons, notifications).
    /// The control's Modulate oscillates via sine wave until stopped.
    /// </summary>
    public void StartPulse(Control control)
    {
        if (control.Disposed)
            return;
        _pulsingControls[control] = 0f;
    }

    /// <summary>
    /// Stop pulsing a control and reset its modulate.
    /// </summary>
    public void StopPulse(Control control)
    {
        if (_pulsingControls.Remove(control))
            control.Modulate = Color.White;
    }
}
