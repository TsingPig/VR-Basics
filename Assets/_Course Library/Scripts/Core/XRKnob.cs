﻿using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using VRExplorer;

/// <summary>
/// An interactable knob that follows the rotation of the interactor
/// </summary>
public class XRKnob : XRBaseInteractable, ITransformableEntity
{


    [ExcludeFromCodeCoverage] public float TriggeringTime => 2.5f;
    [ExcludeFromCodeCoverage] public string Name => Str.Transformable;

    [ExcludeFromCodeCoverage]
    public void Triggerring()
    {
        var obj = EntityManager.Instance.vrexplorerMono.gameObject;
        XRDirectInteractor interactor;
        if(!obj.TryGetComponent(out interactor))
        {
            interactor = obj.AddComponent<XRDirectInteractor>();
        }
        if(!obj.GetComponent<ActionBasedController>())
        {
            obj.AddComponent<ActionBasedController>();
        }

        var e = new SelectEnterEventArgs() { interactorObject = interactor };
        StartTurn(e);
        selectEntered.Invoke(e);
    }

    [ExcludeFromCodeCoverage]
    public void Triggerred()
    {
        var obj = EntityManager.Instance.vrexplorerMono.gameObject;
        XRDirectInteractor interactor;
        if(!obj.TryGetComponent(out interactor))
        {
            interactor = obj.AddComponent<XRDirectInteractor>();
        }
        if(!obj.GetComponent<ActionBasedController>())
        {
            obj.AddComponent<ActionBasedController>();
        }

        OnValueChange.Invoke(DeltaRotation.y);
        Angle = FindRotationValue();
        float finalRotation = ApplyRotation(Angle);

        SetValue(finalRotation);
        selectRotation = selectInteractor.transform.rotation;
        var e = new SelectExitEventArgs() { interactorObject = interactor };
        EndTurn(e);
        selectExited.Invoke(e);
    }

    [ExcludeFromCodeCoverage] public Vector3 DeltaPosition => new Vector3(0, 0, 0);

    [ExcludeFromCodeCoverage] public Vector3 DeltaRotation => new Vector3(0, 180, 0);

    [ExcludeFromCodeCoverage] public Vector3 DeltaScale => new Vector3(0, 0, 0);

    [Tooltip("The transform of the visual component of the knob")]
    public Transform knobTransform = null;

    [Tooltip("The minimum range the knob can rotate")]
    [Range(-180, 0)] public float minimum = -90.0f;

    [Tooltip("The maximum range the knob can rotate")]
    [Range(0, 180)] public float maximum = 90.0f;

    [Tooltip("The initial value of the knob")]
    [Range(0, 1)] public float defaultValue = 0.0f;

    [Serializable] public class ValueChangeEvent : UnityEvent<float> { }

    // When the knobs's value changes
    public ValueChangeEvent OnValueChange = new ValueChangeEvent();

    public float Value { get; private set; } = 0.0f;
    public float Angle { get; private set; } = 0.0f;

    private IXRSelectInteractor selectInteractor = null;
    private Quaternion selectRotation = Quaternion.identity;

    private void Start()
    {
        float defaultRotation = Mathf.Lerp(minimum, maximum, defaultValue);
        ApplyRotation(defaultRotation);
        SetValue(defaultRotation);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        selectEntered.AddListener(StartTurn);
        selectExited.AddListener(EndTurn);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        selectEntered.RemoveListener(StartTurn);
        selectExited.RemoveListener(EndTurn);
    }

    private void StartTurn(SelectEnterEventArgs eventArgs)
    {
        selectInteractor = eventArgs.interactorObject;
        selectRotation = selectInteractor.transform.rotation;
    }

    private void EndTurn(SelectExitEventArgs eventArgs)
    {
        selectInteractor = null;
        selectRotation = Quaternion.identity;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                Angle = FindRotationValue();
                float finalRotation = ApplyRotation(Angle);

                SetValue(finalRotation);
                selectRotation = selectInteractor.transform.rotation;
            }
        }
    }

    private float FindRotationValue()
    {
        Quaternion rotationDifference = selectInteractor.transform.rotation * Quaternion.Inverse(selectRotation);
        Vector3 rotatedForward = rotationDifference * knobTransform.forward;
        return (Vector3.SignedAngle(knobTransform.forward, rotatedForward, transform.up));
    }

    private float ApplyRotation(float angle)
    {
        Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.up);
        newRotation *= knobTransform.localRotation;

        Vector3 eulerRotation = newRotation.eulerAngles;
        eulerRotation.y = ClampAngle(eulerRotation.y);

        knobTransform.localEulerAngles = eulerRotation;
        return eulerRotation.y;
    }

    private float ClampAngle(float angle)
    {
        if (angle > 180)
            angle -= 360;

        return (Mathf.Clamp(angle, minimum, maximum));
    }

    private void SetValue(float rotation)
    {
        Value = Mathf.InverseLerp(minimum, maximum, rotation);
        OnValueChange.Invoke(Value);
    }
}