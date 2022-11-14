using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UILayer.Helpers
{
    public static class ImagePaths
    {
        public static List<String> btnStepBack = new List<string>   {       ImagesPathsString.Instance.Icons_19_11_back_01_01String,
                                                                            ImagesPathsString.Instance.Icons_19_11_back_01_02String
                                                                    };
        public static readonly Geometry btnStepBackGeometry = ImagesPathsString.Instance.GeomCombine(btnStepBack);

        public static List<String> icons_19_11_forward_01_list = new List<string> {   ImagesPathsString.Instance.Icons_19_11_forward_01_01String,
                                                                            ImagesPathsString.Instance.Icons_19_11_forward_01_02String};
        public static readonly Geometry btnStepForwardGeometry = ImagesPathsString.Instance.GeomCombine(icons_19_11_forward_01_list);

        public static readonly Geometry btnGoToEnd = Geometry.Parse(ImagesPathsString.Instance.PlayBack_01String);
        public static readonly Geometry btnGoToStart = Geometry.Parse(ImagesPathsString.Instance.PlayForwardUntilEnd_01String);

    }



    public sealed class ImagesPathsString
    {

        private static ImagesPathsString instance = null;
        public static ImagesPathsString Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ImagesPathsString();
                }
                return instance;
            }
        }

        private ImagesPathsString()
        {
        }
        public string PlayBack_01String = ("m323.332031 26.625c-3.863281-2.25-8.636719-2.25-12.5 0l-281.625 162.5c-3.359375 1.96875-5.621093 5.386719-6.125 9.25v-185.875c0-6.90625-5.59375-12.5-12.5-12.5-6.902343 0-12.5 5.59375-12.5 12.5v375c0 6.90625 5.597657 12.5 12.5 12.5 6.90625 0 12.5-5.59375 12.5-12.5v-186c.46875 3.875 2.742188 7.304688 6.125 9.25l281.625 162.625c3.886719 2.167969 8.617188 2.167969 12.5 0 3.878907-2.246094 6.261719-6.390625 6.25-10.875v-325.125c.039063-4.453125-2.359375-8.578125-6.25-10.75zm-18.75 314.375-244.125-141 244.125-141zm0 0");


        public string Icons_19_11_back_01_01String = ("M4.79,4.19h-1a1.7,1.7,0,0,0-1.7,1.7v18.6a1.7,1.7,0,0,0,1.7,1.7h1a1.7,1.7,0,0,0,1.7-1.7V5.89A1.7,1.7,0,0,0,4.79,4.19Zm.7,20.3a.7.7,0,0,1-.7.7h-1a.71.71,0,0,1-.7-.7V5.89a.7.7,0,0,1,.7-.7h1a.7.7,0,0,1,.7.7Z");
        public string Icons_19_11_back_01_02String = ("M26.07,4.52a1.43,1.43,0,0,0-.73.2L9.63,14a1.44,1.44,0,0,0,0,2.48l15.7,9.23a1.44,1.44,0,0,0,2.17-1.23V6A1.44,1.44,0,0,0,26.07,4.52Zm.44,19.91a.44.44,0,0,1-.44.43.43.43,0,0,1-.22-.06L10.14,15.57a.44.44,0,0,1,0-.76L25.85,5.58a.43.43,0,0,1,.22-.06.44.44,0,0,1,.44.43Z");

        public string Icons_19_11_forward_01_01String = ("M26,4H25a1.7,1.7,0,0,0-1.7,1.7V24.3A1.7,1.7,0,0,0,25,26h1a1.7,1.7,0,0,0,1.7-1.7V5.7A1.7,1.7,0,0,0,26,4Zm.7,20.3a.7.7,0,0,1-.7.7H25a.7.7,0,0,1-.7-.7V5.7A.7.7,0,0,1,25,5h1a.7.7,0,0,1,.7.7Z");
        public string Icons_19_11_forward_01_02String = ("M20.16,13.76,4.45,4.53A1.43,1.43,0,0,0,2.29,5.76V24.24a1.43,1.43,0,0,0,2.16,1.23l15.71-9.23a1.44,1.44,0,0,0,0-2.48Zm-.51,1.62L4,24.61a.46.46,0,0,1-.22.06.44.44,0,0,1-.44-.43V5.76a.44.44,0,0,1,.44-.43A.46.46,0,0,1,4,5.39l15.7,9.23A.44.44,0,0,1,19.65,15.38Z");

        public string PlayForwardUntilEnd_01String = ("m317.082031 0c-6.894531.0195312-12.480469 5.605469-12.5 12.5v186c-.46875-3.875-2.738281-7.304688-6.121093-9.25l-281.628907-162.625c-3.859375-2.25-8.636719-2.25-12.5 0-3.875 2.246094-6.257812 6.390625-6.25 10.875v325.25c-.007812 4.480469 2.375 8.625 6.25 10.875 3.886719 2.164062 8.613281 2.164062 12.5 0l281.628907-162.625c3.359374-1.972656 5.625-5.382812 6.121093-9.25v185.75c0 6.90625 5.597657 12.5 12.5 12.5 6.90625 0 12.5-5.59375 12.5-12.5v-375c-.019531-6.894531-5.605469-12.4804688-12.5-12.5zm-294 341v-282l244.128907 141zm0 0");

        public Geometry GeomCombine(List<String> listGeometryStrings)
        {
            GeometryGroup geometryReturning = new GeometryGroup();
            foreach (var item in listGeometryStrings)
            {
                geometryReturning.Children.Add(Geometry.Parse(item));
            }
            return geometryReturning;
        }

    }
}
