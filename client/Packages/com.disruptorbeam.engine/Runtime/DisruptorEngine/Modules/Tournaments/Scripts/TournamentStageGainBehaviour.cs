﻿using System.Collections;
using System.Collections.Generic;
using DisruptorBeam.Modules.Tournaments;
using UnityEngine;
using UnityEngine.UI;

namespace DisruptorBeam.Modules.Tournaments
{
    [System.Serializable]
    public struct TournamentStageGainDefinition
    {
        public int StageGain;
        public TournamentStageGainBehaviour Prefab;
    }

    public class TournamentStageGainBehaviour : MonoBehaviour
    {
        public List<Image> ChevronImages;
        public Material GreyMaterial;


        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetEffect(bool useGrey)
        {
            // no-op. No longer needed.
        }
    }
}
