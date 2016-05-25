// This causes a force in the "downward" direction of the leg once it touches ground and there is energy.

using System;
using System.Collections.Generic;
using UnityEngine;



public class HL_SQUID_Module : PartModule
{
    #region Variables
    public enum Field { Pull = -1, Push = 1 };
    public enum Direction { Normal, PartRight, PartForward, PartUp, VesselRight, VesselForward, VesselUp };
    private Vector3 ForceDirectionVector;
    private List<ContactPoint> contactList = new List<ContactPoint>();
    private double EnergyDrainFactor;

    // Sound
    public AudioSource SQUID_Sound;
    #endregion

    #region KSPFields
    [KSPField(guiActive = false)]
    public float SQUID_ForceMax = 1500, SQUID_ForceMin = 0, EnergyDrainFactorF = 0.0001F, SoundVolume = 1f;
    // Unfortunately, KSPFields can't handle doubles

    [KSPField(isPersistant = true, guiActive = true)]
    public bool SQUID_On = true, onlyGround = false;

    [KSPField(isPersistant = true, guiActive = true)]
    public float SQUID_Force = 1f;

    [KSPField(isPersistant = true, guiActive = true)]
    public Field ForceField = Field.Pull;

    [KSPField(isPersistant = true, guiActive = true)]
    public Direction ForceDirection = Direction.Normal;
    #endregion

    #region KSPEvents for all SQUID parts on the vessel
    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "SQUID Toggle All")]
    public void SQUID_Toggle_All()
    {
        part.SendEvent("SQUID_Toggle");
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Push / Pull All")]
    public void Push_Pull_All()
    {
        part.SendEvent("Push_Pull");
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Force Increase All")]
    public void Force_Increase_All()
    {
        part.SendEvent("Force_Increase");
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Force Reduce All")]
    public void Force_Reduce_All()
    {
        part.SendEvent("Force_Reduce");
    }

    // Remember to update this if direction enum changes!
    
    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Force Direction All")]
    public void Force_Direction_All()
    {
        part.SendEvent("Force_Direction");
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Only Affects Ground All")]
    public void Only_Ground_All()
    {
        part.SendEvent("Only_Ground");
    }
#endregion

    #region KSPActions
    [KSPAction("SQUID Toggle")]
    public void SQUID_Toggle_Action(KSPActionParam param)
    {
        SQUID_Toggle();
    }

    [KSPAction("Push / Pull")]
    public void Push_Pull_Action(KSPActionParam param)
    {
        Push_Pull();
    }

    [KSPAction("Force Increase")]
    public void Force_Increase_Action(KSPActionParam param)
    {
        Force_Increase();
    }
    
    [KSPAction("Force Reduce")]
    public void Force_Reduce_Action(KSPActionParam param)
    {
        Force_Reduce();
    }

    [KSPAction("Force Direction")]
    public void Force_Direction_Action(KSPActionParam param)
    {
        Force_Direction();
    }

    [KSPAction("Only Affects Ground")]
    public void Only_Ground_Action(KSPActionParam param)
    {
        Only_Ground();
    }
    #endregion

    #region KSPEvents for individual parts
    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName="SQUID Toggle")]
    public void SQUID_Toggle()
    {
        if (SQUID_On) SQUID_On = false;
        else SQUID_On = true;
    }

    
    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Push / Pull")]
    public void Push_Pull()
    {
        if (ForceField == Field.Pull) ForceField = Field.Push;
        else ForceField = Field.Pull;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Force Increase")]
    public void Force_Increase()
    {
        SQUID_Force += 10;
    }

    
    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Force Reduce")]
    public void Force_Reduce()
    {
        SQUID_Force -= 10;
    }

    // Remember to update this if direction enum changes!
    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Force Direction")]
    public void Force_Direction()
    {
        // For safety, disable force
        SQUID_On = false;

        // Change direction
        if ((int)ForceDirection > 0) ForceDirection -= 1;
        else ForceDirection = Direction.VesselUp;
    }

    [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 2000, guiName = "Only Affects Ground")]
    public void Only_Ground()
    {
        if (onlyGround) onlyGround = false;
        else onlyGround = true;
    }
    #endregion

    public void OnCollisionStay(Collision collision)
    {
        contactList.Clear();

        foreach (ContactPoint contactThis in collision.contacts)
        {
            contactList.Add(contactThis);
            // Debug.Log("Added contact " + contactThis.point.x + " " + contactThis.point.y + " " + contactThis.point.z);
        }
    }

    public void Start()
    {
        // Convert energy drain factor from single (readable by KSPfield) to double (for calculations)
        EnergyDrainFactor = EnergyDrainFactorF;

        // Load sound
        SQUID_Sound = part.gameObject.AddComponent<AudioSource>();
        Debug.Log("Added SQUID_Sound");
        Debug.Log("Path is " + "file://" + KSPUtil.ApplicationRootPath + "sounds/sound_HL_SQUID_Hum.wav");
        WWW w = new WWW("file://" + KSPUtil.ApplicationRootPath + "sounds/sound_HL_SQUID_Hum.wav");

        SQUID_Sound.clip = w.GetAudioClip(true);
        Debug.Log("Clip loaded");
        SQUID_Sound.loop = true;
        SQUID_Sound.playOnAwake = false;
        SQUID_Sound.volume = SoundVolume;
        SQUID_Sound.maxDistance = 1;
    }
    
    public void Update()
    {
        // Make sure the part is active
        if (HighLogic.LoadedSceneIsFlight && !vessel.packed)
        {
            if (part.State != PartStates.ACTIVE) part.force_activate();
        }

        if (SQUID_Sound.isPlaying && PauseMenu.isOpen)
        {
            SQUID_Sound.Stop();
            Debug.Log("Stopping SQUID Hum");
        }
    }
    
    public void FixedUpdate()
    {
        try { Paramagnetism(); }
        catch (Exception ex) { print("Paramagnetism Exception!"); print(ex.Message); }
        
        // vessel.printGroundContacts();
    }

    public void Paramagnetism()
    {
        //vessel.printCollisions();
        
        // Clamp force
        SQUID_Force = Mathf.Clamp(SQUID_Force, SQUID_ForceMin, SQUID_ForceMax);
        
        foreach (ContactPoint contactHere in contactList)
        {
            Debug.Log("Contact Point with " + contactHere.otherCollider.gameObject.name);
            //if (contactHere.otherCollider == part.parent.collider)
            //    return;

            // If the mode is not "normal" then overwrite with the direction
            switch (ForceDirection)
            {
                case Direction.Normal:
                    ForceDirectionVector = contactHere.normal;
                    break;
                
                case Direction.PartForward:
                    ForceDirectionVector = part.transform.forward;
                    break;

                case Direction.PartRight:
                    ForceDirectionVector = part.transform.right;
                    break;

                case Direction.PartUp:
                    ForceDirectionVector = part.transform.up;
                    break;

                case Direction.VesselForward:
                    ForceDirectionVector = vessel.transform.forward;
                    break;

                case Direction.VesselRight:
                    ForceDirectionVector = vessel.transform.right;
                    break;

                case Direction.VesselUp:
                    ForceDirectionVector = vessel.transform.up;
                    break;
            }

            if (SQUID_On && (part.GroundContact || !onlyGround) && part.currentCollisions.Count > 0 && !vessel.HoldPhysics)
            {
                // Debug.Log("There is contact with " + part.name);

                // Apply force if there is electricity
                if (part.RequestResource("ElectricCharge", EnergyDrainFactor * SQUID_Force) > 0)
                {
                    // Debug.Log("Applying SQUID Powers with " + part.name + "!");
                    //part.Rigidbody.AddForceAtPosition(ForceDirectionVector * SQUID_Force * (int)ForceField, contactHere.point, ForceMode.Force);

                    //// Animation adapted from Tosh's antigrav module
                    //GameObject o = Instantiate(
                    //UnityEngine.Resources.Load("Effects/fx_exhaustFlame_blue")) as GameObject;
                    //o.transform.position = contactHere.point;
                    //o.transform.rotation = Quaternion.identity;
                    //// o.transform.localPosition = Vector3.zero;

                    //ParticleEmitter emt = o.AddComponent<ParticleEmitter>();
                    //emt.emit = true;
                    //emt.useWorldSpace = true;
                    //emt.minEnergy = 1F;
                    //emt.maxEnergy = 2F;
                    //emt.minEmission = SQUID_Force * 0.5F;
                    //emt.maxEmission = SQUID_Force * 1.0F;
                    //emt.minSize = 0.1F * 0.5f;
                    //emt.maxSize = 0.2F * 0.5f;
                    //emt.rndVelocity = (Vector3.one + (0.9f * ForceDirectionVector.normalized)) * 0.5f;
                    //emt.localVelocity = Vector3.zero;
                    //(o.GetComponent<ParticleAnimator>() as ParticleAnimator).sizeGrow = 0;
                    //Destroy(o, 2.0f);

                    // Equal and opposite force
                    if (contactHere.otherCollider.attachedRigidbody != null)
                    {
                        contactHere.otherCollider.attachedRigidbody.AddForceAtPosition(-1 * ForceDirectionVector * SQUID_Force * (int)ForceField, contactHere.point, ForceMode.Force);
                        // Debug.Log("Added force to collided object, " + contactHere.otherCollider.attachedRigidbody.gameObject.name);
                        // Debug.Log("Force direction: " + (-1 * ForceDirectionVector * SQUID_Force * (int)ForceField).x + " " + (-1 * ForceDirectionVector * SQUID_Force * (int)ForceField).y + " " + (-1 * ForceDirectionVector * SQUID_Force * (int)ForceField).z);
                        // Debug.Log("Point: " + contactHere.point.x + " " + contactHere.point.y + " " + contactHere.point.z);
                    }

                    // Play sound, based on ModuleAnimation by Kreuzung
                    if (SQUID_Sound != null && !SQUID_Sound.isPlaying)
                    {
                        SQUID_Sound.Play();
                        // Debug.Log("Starting SQUID Hum");
                    }
                }
            }
            else if (SQUID_Sound.isPlaying)
            {
                SQUID_Sound.Stop();
                // Debug.Log("Stopping SQUID Hum");
            }
        }
    }
}

