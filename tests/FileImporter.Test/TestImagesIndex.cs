using System.Collections.Generic;
using EagleEye.FileImporter.Indexing;
using EagleEye.FileImporter.Json;

namespace EagleEye.FileImporter.Test
{
    public static class TestImagesIndex
    {
        public static string IndexJson { get; } = @"[
  {
    ""Identifier"": ""1.jpg"",
    ""Hashes"": {
      ""FileHash"": ""++w{S2[kj437J2P(i{waAoqjvk1S)wE%vL3@fv0O"",
      ""ImageHash"": ""+]JP}/TH1-hP/ax&a)iqy%H<Ze>cpbBhph)ggipp"",
      ""AverageHash"": 18446717683803819524,
      ""DifferenceHash"": 3573764261290621052,
      ""PerceptualHash"": 15585629762494286247
    }
  },
  {
    ""Identifier"": ""1wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""qtX]SB9l<WOiqsA-QV%w:x#*TG?Z^Z<{&(hN2.D&"",
      ""ImageHash"": ""B)}t0Hj5h<bx6G&popv-H1SGDfe%IZlY*iNlF.Q:"",
      ""AverageHash"": 18446717683803819524,
      ""DifferenceHash"": 3573764261290621052,
      ""PerceptualHash"": 15585629762494286247
    }
  },
  {
    ""Identifier"": ""2.jpg"",
    ""Hashes"": {
      ""FileHash"": ""]{1kaSsfj*uo6Wv0c[-*Nb/wQ:kGT2Pr/8@4{ZM$"",
      ""ImageHash"": ""M:c%N?8U3h^#bL]8iYmANd<AQ4l$mTgf1wzhOM>x"",
      ""AverageHash"": 4093965717081112447,
      ""DifferenceHash"": 17340674709071853308,
      ""PerceptualHash"": 11979645924150615927
    }
  },
  {
    ""Identifier"": ""2wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""cwWMITr}>e4%3K2M&#i:eL75fFWcfA5^t>QkE)jQ"",
      ""ImageHash"": ""U7DFb)YE.6qdW!d?Gd+jCWt:MZYYloquS}T3TjhJ"",
      ""AverageHash"": 4093965717081112447,
      ""DifferenceHash"": 17340674709073950462,
      ""PerceptualHash"": 11979645924150615927
    }
  },
  {
    ""Identifier"": ""3.jpg"",
    ""Hashes"": {
      ""FileHash"": ""^o0*buZsr?<}J8LrZWAtLNB#ZJ+w@h?0bCZq3Azh"",
      ""ImageHash"": ""AHbguvVf]$li1$@0[vPy&&m{OPhH%7liQE{PzyM?"",
      ""AverageHash"": 1746901995957518336,
      ""DifferenceHash"": 17327732025059362952,
      ""PerceptualHash"": 13994443045429955730
    }
  },
  {
    ""Identifier"": ""3wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""}LI4>A*eQ&y2KAwzOs^/MfG=(WY[H?2Ld%(IxfZ@"",
      ""ImageHash"": ""+MkKRaZ*<}:rdIr8w1<il8E*D{[BxBFJB-7?u:+x"",
      ""AverageHash"": 1746901995957518336,
      ""DifferenceHash"": 17327872762547718296,
      ""PerceptualHash"": 13994443045429955730
    }
  },
  {
    ""Identifier"": ""4.jpg"",
    ""Hashes"": {
      ""FileHash"": ""-!VJQj34vHI!aXB6@iU14$E}jf:rV<s#6btzfvqh"",
      ""ImageHash"": ""?txyvr6FtM-#1wSRX*LW}VYjwN$zT??0=k)JR7#c"",
      ""AverageHash"": 3474298655623479295,
      ""DifferenceHash"": 7477312801918790912,
      ""PerceptualHash"": 11839175152576027866
    }
  },
  {
    ""Identifier"": ""4wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""}6FemJ<HHc@>B.eGks^=iWxbe[/W}z3dlcNPPy2u"",
      ""ImageHash"": ""-qOvW[ol6Zwo9go*umUX]k?Y5/fdlP?HW+No:Ceh"",
      ""AverageHash"": 3474298655623479295,
      ""DifferenceHash"": 7477875751872211200,
      ""PerceptualHash"": 11839175152576027866
    }
  },
  {
    ""Identifier"": ""6.jpg"",
    ""Hashes"": {
      ""FileHash"": ""Y4Y8aa-Cwe):!?eGPV%&W*Hl./iq?opwu}gHbgl&"",
      ""ImageHash"": ""0viV^gr2<h)f(^O:3FG#q=C%[+4ujyD6kKH1l}YL"",
      ""AverageHash"": 4608497811455,
      ""DifferenceHash"": 130174362314118373,
      ""PerceptualHash"": 9456493324026014833
    }
  },
  {
    ""Identifier"": ""6wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""lx9y(@oZemtcI6a3-P!4@K)!$f(!&Pl4dKN2MGZ9"",
      ""ImageHash"": "":U4D}})-noD/gX-5UKHl)@4aeR(2b=Aczhy3j6&/"",
      ""AverageHash"": 4608497811455,
      ""DifferenceHash"": 130174362314118885,
      ""PerceptualHash"": 9456493324026014833
    }
  },
  {
    ""Identifier"": ""7.jpg"",
    ""Hashes"": {
      ""FileHash"": ""4i6fE)3Zd#d7p]WlGG+2S#vMg4$0z[tR?FkgDQwe"",
      ""ImageHash"": ""><*>7N<.hpT2o13(Q@6kZT8=msafB-b0A![G)8CZ"",
      ""AverageHash"": 4453327437823,
      ""DifferenceHash"": 421364555712865764,
      ""PerceptualHash"": 9456493632260892232
    }
  },
  {
    ""Identifier"": ""7wa.jpg"",
    ""Hashes"": {
      ""FileHash"": "":qke.<P2<(=62Ng(RKnyCqAyEAjL8]e]lw}qUn^7"",
      ""ImageHash"": ""hbYr540F9e8S:oy+@?=B6sEE}2qcj*d}<Yzvqht?"",
      ""AverageHash"": 4453327437823,
      ""DifferenceHash"": 421364555712882148,
      ""PerceptualHash"": 9456493632260892232
    }
  },
  {
    ""Identifier"": ""8.jpg"",
    ""Hashes"": {
      ""FileHash"": ""dWHP5sX<p@Mc4a93y.W]Bw[epBbq$%{T8rV(Y=)!"",
      ""ImageHash"": ""7&yEPhk(r&.gUetd@oQquZNPC:qVs^gMQkKtSIYz"",
      ""AverageHash"": 18012702927131052843,
      ""DifferenceHash"": 11790867287640325097,
      ""PerceptualHash"": 17998763484001462542
    }
  }
]";

        public static List<ImageData> Index { get; } = JsonEncoding.Deserialize<List<ImageData>>(IndexJson);
    }
}