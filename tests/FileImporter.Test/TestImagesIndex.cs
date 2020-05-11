namespace EagleEye.FileImporter.Test
{
    using System.Collections.Generic;

    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Json;

    public static class TestImagesIndex
    {
        public static string IndexJson { get; } = @"[
  {
    ""Identifier"": ""1.jpg"",
    ""Hashes"": {
      ""FileHash"": ""++w{S2[kj437J2P(i{waAoqjvk1S)wE%vL3@fv0O"",
      ""ImageHash"": ""+]JP}/TH1-hP/ax&a)iqy%H<Ze>cpbBhph)ggipp"",
      ""AverageHash"": 18442214084176449028,
      ""DifferenceHash"": 3573764330010097788,
      ""PerceptualHash"": 15585629762494286247
    }
  },
  {
    ""Identifier"": ""1wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""qtX]SB9l<WOiqsA-QV%w:x#*TG?Z^Z<{&(hN2.D&"",
      ""ImageHash"": ""]+:uJ=I=5{etSH@U3orh6g3YYtL%K*FIpSyjDH%Z"",
      ""AverageHash"": 18442214084176449028,
      ""DifferenceHash"": 3573764330010097788,
      ""PerceptualHash"": 15585629762494286247
    }
  },
  {
    ""Identifier"": ""2.jpg"",
    ""Hashes"": {
      ""FileHash"": ""]{1kaSsfj*uo6Wv0c[-*Nb/wQ:kGT2Pr/8@4{ZM$"",
      ""ImageHash"": ""M:c%N?8U3h^#bL]8iYmANd<AQ4l$mTgf1wzhOM>x"",
      ""AverageHash"": 4093965717081112447,
      ""DifferenceHash"": 16187753204465006332,
      ""PerceptualHash"": 11979645924150615927
    }
  },
  {
    ""Identifier"": ""2wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""cwWMITr}>e4%3K2M&#i:eL75fFWcfA5^t>QkE)jQ"",
      ""ImageHash"": ""ds=T!^*c*gVR^IEe2t9kF34xtvt[aYQE&3rXL-cS"",
      ""AverageHash"": 4093965717081112447,
      ""DifferenceHash"": 17628905085225662206,
      ""PerceptualHash"": 11979645924150615927
    }
  },
  {
    ""Identifier"": ""3.jpg"",
    ""Hashes"": {
      ""FileHash"": ""^o0*buZsr?<}J8LrZWAtLNB#ZJ+w@h?0bCZq3Azh"",
      ""ImageHash"": ""Vgm-Kw0bWG*d*O(LG2IkIfWCK)zP8QSwp^*yfrS("",
      ""AverageHash"": 1746901995957518336,
      ""DifferenceHash"": 17327732025059362952,
      ""PerceptualHash"": 13994443045429955730
    }
  },
  {
    ""Identifier"": ""3wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""}LI4>A*eQ&y2KAwzOs^/MfG=(WY[H?2Ld%(IxfZ@"",
      ""ImageHash"": ""mRD=z^f7NG0.}^VrGox:UkwJA.eRtzBhl%(pz]]S"",
      ""AverageHash"": 1746901995957518336,
      ""DifferenceHash"": 17363901559566682248,
      ""PerceptualHash"": 13994443045429955730
    }
  },
  {
    ""Identifier"": ""4.jpg"",
    ""Hashes"": {
      ""FileHash"": ""-!VJQj34vHI!aXB6@iU14$E}jf:rV<s#6btzfvqh"",
      ""ImageHash"": ""?txyvr6FtM-#1wSRX*LW}VYjwN$zT??0=k)JR7#c"",
      ""AverageHash"": 3474298655623479295,
      ""DifferenceHash"": 7477875751872212224,
      ""PerceptualHash"": 12415618312693406938
    }
  },
  {
    ""Identifier"": ""4wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""}6FemJ<HHc@>B.eGks^=iWxbe[/W}z3dlcNPPy2u"",
      ""ImageHash"": ""{*w4paqPoAZNbdg?krEe]7Z=n4I8no4UFqMSm)@y"",
      ""AverageHash"": 3474298655623479295,
      ""DifferenceHash"": 7477875751872211200,
      ""PerceptualHash"": 12415618312693406938
    }
  },
  {
    ""Identifier"": ""6.jpg"",
    ""Hashes"": {
      ""FileHash"": ""Y4Y8aa-Cwe):!?eGPV%&W*Hl./iq?opwu}gHbgl&"",
      ""ImageHash"": ""0viV^gr2<h)f(^O:3FG#q=C%[+4ujyD6kKH1l}YL"",
      ""AverageHash"": 210451300351,
      ""DifferenceHash"": 130174362314118373,
      ""PerceptualHash"": 9456493324026080337
    }
  },
  {
    ""Identifier"": ""6wa.jpg"",
    ""Hashes"": {
      ""FileHash"": ""lx9y(@oZemtcI6a3-P!4@K)!$f(!&Pl4dKN2MGZ9"",
      ""ImageHash"": ""@fTPqlxQVK{T5@.(*:9<ir19({[+#GzfgOn+2bgt"",
      ""AverageHash"": 210451300351,
      ""DifferenceHash"": 130174362314118885,
      ""PerceptualHash"": 9456493324026080337
    }
  },
  {
    ""Identifier"": ""7.jpg"",
    ""Hashes"": {
      ""FileHash"": ""4i6fE)3Zd#d7p]WlGG+2S#vMg4$0z[tR?FkgDQwe"",
      ""ImageHash"": ""><*>7N<.hpT2o13(Q@6kZT8=msafB-b0A![G)8CZ"",
      ""AverageHash"": 4453327437823,
      ""DifferenceHash"": 421364555712865764,
      ""PerceptualHash"": 9456489234212284012
    }
  },
  {
    ""Identifier"": ""7wa.jpg"",
    ""Hashes"": {
      ""FileHash"": "":qke.<P2<(=62Ng(RKnyCqAyEAjL8]e]lw}qUn^7"",
      ""ImageHash"": ""spuPWeDQ13F*dpWwqXlB+cFXIEvY!]QV}DQ9RKdK"",
      ""AverageHash"": 4453327437823,
      ""DifferenceHash"": 421364555712882148,
      ""PerceptualHash"": 9456489234212284012
    }
  },
  {
    ""Identifier"": ""8.jpg"",
    ""Hashes"": {
      ""FileHash"": ""dWHP5sX<p@Mc4a93y.W]Bw[epBbq$%{T8rV(Y=)!"",
      ""ImageHash"": ""7&yEPhk(r&.gUetd@oQquZNPC:qVs^gMQkKtSIYz"",
      ""AverageHash"": 18012702927131052843,
      ""DifferenceHash"": 11790867279050390505,
      ""PerceptualHash"": 17998763484001462542
    }
  }
]";

        public static List<ImageData> Index { get; } = JsonEncoding.Deserialize<List<ImageData>>(IndexJson);
    }
}
