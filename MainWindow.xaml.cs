using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace secretMessageDecoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }//end main

        //GLOBAL VARS
        BitmapMaker _loadedImage;
        string _ImageFilePath;        
        int _height;
        int _width;
        string _idHeader;
        byte[] _p6Array;        
        string[] _p3Array;
        int _index = 0;
        int _pixelBytes;       
        int cipherNumber = 0;

        private void Decode_Click(object sender, RoutedEventArgs e) {
            Decode();
        }//end Encode Click Event
        private void btnDecodeMessage_Click(object sender, RoutedEventArgs e) {
            Decode();
        }
        private void LoadImage_Click(object sender, RoutedEventArgs e) {
            //openfiledailog to open the file you want
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                _ImageFilePath = openFileDialog.FileName;

            }//end if

            //read all lines of the file and save to a string array
            _p3Array = File.ReadAllLines(_ImageFilePath);

            //read through the first lines of the file and pull relavent info from them 
            ReadHeaders();

            if (_idHeader == "P3") {//for a P3 file

                SetP3Pixels();

                LoadImage();
                
            }//end if
            if (_idHeader == "P6") {//for a P6 file
                //reads and saves text file as a byte array
                _p6Array = File.ReadAllBytes(_ImageFilePath);
                               
                SetP6Pixels();

                LoadImage();

            }//end if

            //once image is loaded various boxes appear on the form
            if (imgLoadedImage.IsLoaded) {
                lblCipherNumber.Visibility = Visibility.Visible;
                txtCipherNumber.Visibility = Visibility.Visible;
                btnDecodeMessage.Visibility = Visibility.Visible;      
            }//end if

        }//end LoadImage Click Event
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }//end Exit Click Event
        private void SetP6Pixels() {
            int index = _p6Array.Length - _pixelBytes;
            //set pixel data for the image
            for (int y = 0; y < _height; y++) {
                for (int x = 0; x < _width; x++) {
                    _loadedImage.SetPixel(x, y, _p6Array[index], _p6Array[index + 1], _p6Array[index + 2]);
                    index += 3;
                }//end for
            }//end for
        }//end SetP6Pixels
        private void SetP3Pixels()
        {
            _index = _p3Array.Length - _pixelBytes;
            //set pixel data for the image
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _loadedImage.SetPixel(x, y, byte.Parse(_p3Array[_index]), byte.Parse(_p3Array[_index + 1]), byte.Parse(_p3Array[_index + 2]));
                    _index += 3;
                }//end for
            }//end for
        }//end SetP3Pixels   
        private void ReadHeaders()
        {        
            _index = 0;
            //read first line of file to get the type
            _idHeader = _p3Array[_index];
            _index += 1;

            //store second line of file which should be a comment 
            string record = _p3Array[_index];

            while (record[0] == '#')
            {//read throught all the comments
                record = _p3Array[_index];
                _index += 1;
            }//end while

            //split the next line to get the width and height of the image
            string[] fields = record.Split(' ');
            _width = int.Parse(fields[0]);
            _height = int.Parse(fields[1]);

            //get number of bytes in the pixels minus the alpha value from each pixel
            _pixelBytes = _height * _width * 3;

            //skip next line which is the max rgb channel           
            record = _p3Array[_index];
            _index += 1;

            //set up a new bitmap maker with the width and height read from the file
            _loadedImage = new BitmapMaker(_width, _height);

        }//end ReadHeaders
        private void LoadImage()
        {
            //convert pixel data into writable bitmap
            WriteableBitmap wbmImage = _loadedImage.MakeBitmap();

            //set nearest neighbor rendering mode for image box
            RenderOptions.SetBitmapScalingMode(imgLoadedImage, BitmapScalingMode.NearestNeighbor);

            //set uniform stretching to scale image cleanly 
            imgLoadedImage.Stretch = Stretch.Uniform;

            //set source for image box to the bitmap
            imgLoadedImage.Source = wbmImage;
        }//end LoadImage
        private void Decode() {
            //keeps the user from trying to decode the message before putting in the cipher
            if (txtCipherNumber.Text == "") {
                throw new Exception("You must enter your cipher number in order to decode the image.");
            }
            txtDecodedMessage.Text = "";

            if (_idHeader == "P3") {
                //read the cipher
                cipherNumber = int.Parse(txtCipherNumber.Text);
                //create an array to hold the message
                string[] decodedMessage = new string[cipherNumber];
                //set the index to the start of the message in the image data
                int index2 = _p3Array.Length - 1;

                //move through the image data while saving the message to an array
                for (int index = 0; index < cipherNumber; index++) {
                    decodedMessage[index] = _p3Array[index2];
                    index2 -= 9;
                }//end for

                //print the message to the text box
                foreach (var item in decodedMessage) {
                    txtDecodedMessage.Text += (char)int.Parse(item);
                }//end for each
            }//end if
            if (_idHeader == "P6") {
                //read the cipher
                cipherNumber = int.Parse(txtCipherNumber.Text);
                //create an array to hold the message
                string[] decodedMessage = new string[cipherNumber];
                //set the index to the start of the message in the image data
                int index2 = _p6Array.Length - 1;

                //move through the image data while saving the message to an array
                for (int index = 0; index < cipherNumber; index++) {
                    decodedMessage[index] = _p6Array[index2].ToString();
                    index2 -= 9;
                }//end for

                //print the message to the text box
                foreach (var item in decodedMessage) {
                    txtDecodedMessage.Text += (char)int.Parse(item);
                }//end foreach
            }//end if
        }//end Decode

        
    }//end class
}//end namespace
