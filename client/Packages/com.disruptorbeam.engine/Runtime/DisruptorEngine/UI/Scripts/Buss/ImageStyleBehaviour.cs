using DisruptorBeam.UI.Buss.Extensions;
using DisruptorBeam.UI.MSDF;
using UnityEngine;

namespace DisruptorBeam.UI.Buss
{
    [ExecuteInEditMode]
    public class ImageStyleBehaviour : StyleBehaviour
    {
        static ImageStyleBehaviour()
        {
            RegisterType<ImageStyleBehaviour>("img");
        }

        public BeamableMSDFBehaviour MsdfBehaviour;

        public override string TypeString => "img";

        public override void Apply(StyleObject styles)
        {
            MsdfBehaviour.ApplyStyleObject(styles);

        }
    }
}
