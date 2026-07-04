// SPDX-FileCopyrightText: 2026 Freak
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Content.Client.UserInterface.Controls;

namespace Content.Client._FreakyStation.UI;

/// <summary>
/// Animated UI transitions. Non-blocking — windows always stay visible regardless of animation state.
/// </summary>
public sealed class UiAnimationSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    // Entrance animation tracking — manual interpolation, no PlayAnimation
    private readonly Dictionary<Control, float> _entranceAnims = new();
    private readonly List<Control> _entranceDone = new();

    // Pulse tracking
    private readonly Dictionary<Control, float> _pulsingControls = new();
    private readonly List<Control> _pulseRemove = new();

    private const float EntranceDuration = 0.25f;
    private const float PulsePeriod = 1.6f;

    // Hover/click use PlayAnimation with try-catch — they're non-critical
    private const string HoverKey = "freaky-hover";
    private const string ClickKey = "freaky-click";
    private const float HoverDuration = 0.12f;
    private const float ClickDuration = 0.15f;

    private static readonly Color HoverTint = new(0.88f, 0.88f, 0.88f, 1f);
    private static readonly Color PressTint = new(0.78f, 0.78f, 0.78f, 1f);

    public override void Initialize()
    {
        base.Initialize();
        _uiManager.WindowRoot.OnChildAdded += OnWindowChildAdded;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateEntrance(frameTime);
        UpdatePulse(frameTime);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _uiManager.WindowRoot.OnChildAdded -= OnWindowChildAdded;
    }

    private void OnWindowChildAdded(Control child)
    {
        if (child is not (DefaultWindow or FancyWindow))
            return;

        // Start manual fade-in — set transparent immediately (before first render)
        child.Modulate = Color.White.WithAlpha(0f);
        _entranceAnims[child] = 0f;
    }

    private void UpdateEntrance(float frameTime)
    {
        foreach (var (control, elapsed) in _entranceAnims)
        {
            if (control.Disposed)
            {
                _entranceDone.Add(control);
                continue;
            }

            var t = Math.Min(elapsed / EntranceDuration, 1f);
            // Ease-out-cubic
            var eased = 1f - MathF.Pow(1f - t, 3f);
            control.Modulate = new Color(1f, 1f, 1f, eased);

            if (t >= 1f)
            {
                control.Modulate = Color.White;
                _entranceDone.Add(control);
            }
            else
            {
                _entranceAnims[control] = elapsed + frameTime;
            }
        }

        foreach (var control in _entranceDone)
            _entranceAnims.Remove(control);
        _entranceDone.Clear();
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

            var pulse = 0.85f + 0.15f * MathF.Sin(t * MathF.Tau / PulsePeriod);
            control.Modulate = new Color(pulse, pulse, pulse, 1f);
        }

        foreach (var control in _pulseRemove)
            _pulsingControls.Remove(control);
        _pulseRemove.Clear();
    }

    public static void AnimateHoverIn(Control control)
    {
        if (control.Disposed)
            return;
        try
        {
            control.StopAnimation(HoverKey);
            control.PlayAnimation(new Robust.Client.Animations.Animation
            {
                Length = TimeSpan.FromSeconds(HoverDuration),
                AnimationTracks =
                {
                    new Robust.Client.Animations.AnimationTrackControlProperty
                    {
                        Property = "Modulate",
                        KeyFrames =
                        {
                            new Robust.Client.Animations.AnimationTrackProperty.KeyFrame(Color.White, 0f),
                            new Robust.Client.Animations.AnimationTrackProperty.KeyFrame(HoverTint, HoverDuration),
                        }
                    },
                }
            }, HoverKey);
        }
        catch { /* ignore */ }
    }

    public static void AnimateHoverOut(Control control)
    {
        if (control.Disposed)
            return;
        try
        {
            control.StopAnimation(HoverKey);
            control.PlayAnimation(new Robust.Client.Animations.Animation
            {
                Length = TimeSpan.FromSeconds(HoverDuration),
                AnimationTracks =
                {
                    new Robust.Client.Animations.AnimationTrackControlProperty
                    {
                        Property = "Modulate",
                        KeyFrames =
                        {
                            new Robust.Client.Animations.AnimationTrackProperty.KeyFrame(HoverTint, 0f),
                            new Robust.Client.Animations.AnimationTrackProperty.KeyFrame(Color.White, HoverDuration),
                        }
                    },
                }
            }, HoverKey);
        }
        catch { /* ignore */ }
    }

    public static void AnimateClickPop(Control control)
    {
        if (control.Disposed)
            return;
        try
        {
            control.StopAnimation(ClickKey);
            control.PlayAnimation(new Robust.Client.Animations.Animation
            {
                Length = TimeSpan.FromSeconds(ClickDuration),
                AnimationTracks =
                {
                    new Robust.Client.Animations.AnimationTrackControlProperty
                    {
                        Property = "Modulate",
                        KeyFrames =
                        {
                            new Robust.Client.Animations.AnimationTrackProperty.KeyFrame(PressTint, 0f),
                            new Robust.Client.Animations.AnimationTrackProperty.KeyFrame(Color.White, ClickDuration),
                        }
                    },
                }
            }, ClickKey);
        }
        catch { /* ignore */ }
    }

    public void StartPulse(Control control)
    {
        if (control.Disposed)
            return;
        _pulsingControls[control] = 0f;
    }

    public void StopPulse(Control control)
    {
        if (_pulsingControls.Remove(control))
            control.Modulate = Color.White;
    }
}
