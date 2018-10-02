using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBotWithCounter.Models
{
    public class FaceResponseDto
    {
        public FaceRectangleDto FaceRectangle { get; set; }

        public FaceAttributesDto FaceAttributes { get; set; }
    }
}
