using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImageListDemo
{
    public partial class Form1 : Form
    {
        // selected image index, from the listview
        private int SelectedImageIndex = 0;
        private List<Image> LoadedImages { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadImagesFromFolder(string[] paths)
        {
            LoadedImages = new List<Image>();
            foreach(var path in paths)
            {
                var tempImage = Image.FromFile(path);
                LoadedImages.Add(tempImage);
            }
        }

        private void imageList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (imageList.SelectedIndices.Count > 0)
            {
                if (imageList.SelectedIndices.Count == 1)
                {
                    // display single selected image
                    var selectedIndex = imageList.SelectedIndices[0];
                    Image selectedImg = LoadedImages[selectedIndex];
                    selectedImage.Image = selectedImg;
                    SelectedImageIndex = selectedIndex;
                }
                else
                {
                    // display multiple selected images
                    List<Image> selectedImages = new List<Image>();
                    foreach (int index in imageList.SelectedIndices)
                    {
                        selectedImages.Add(LoadedImages[index]);
                    }
                    selectedImage.Image = CombineImages(selectedImages);
                    SelectedImageIndex = -1;
                }
            }
        }

        private Image CombineImages(List<Image> images)
        {
            // calculate the combined image size
            int maxImageHeight = images.Max(img => img.Height);
            int imagesPerRow = 0;
            int imagesPerColumn = 0;
            if (images.Count <= 2)
            {
                imagesPerRow = images.Count;
                imagesPerColumn = 1;
            }
            else if (images.Count <= 4)
            {
                imagesPerRow = 2;
                imagesPerColumn = 2;
            }
            else if (images.Count <= 6)
            {
                imagesPerRow = 3;
                imagesPerColumn = 2;
            }
            else if (images.Count <= 8)
            {
                imagesPerRow = 4;
                imagesPerColumn = 2;
            }
            int totalWidth = imagesPerRow * images[0].Width;
            int totalHeight = maxImageHeight * imagesPerColumn;

            // create a new bitmap to hold the combined images
            Bitmap combinedImage = new Bitmap(totalWidth, totalHeight);

            // draw each image onto the combined image
            using (Graphics g = Graphics.FromImage(combinedImage))
            {
                int x = 0, y = 0;
                int imagesDrawn = 0;
                foreach (Image img in images)
                {
                    if (imagesDrawn % imagesPerRow == 0 && imagesDrawn != 0)
                    {
                        x = 0;
                        y += maxImageHeight;
                    }

                    g.DrawImage(img, x, y);
                    x += images[0].Width;
                    imagesDrawn++;
                }
            }

            return combinedImage;
        }


        private void button_navigation(object sender, EventArgs e)
        {
            var clickedButton = sender as Button;
            if (clickedButton.Text.Equals("Previous"))
            {
                if (SelectedImageIndex > 0)
                {
                    SelectedImageIndex -= 1;
                    Image selectedImg = LoadedImages[SelectedImageIndex];
                    selectedImage.Image = selectedImg;
                    SelectTheClickedItem(imageList, SelectedImageIndex);
                }

            } else
            {
                if (SelectedImageIndex < (LoadedImages.Count - 1 ))
                {
                    SelectedImageIndex += 1;
                    Image selectedImg = LoadedImages[SelectedImageIndex];
                    selectedImage.Image = selectedImg;
                    SelectTheClickedItem(imageList, SelectedImageIndex);
                }
            }
        }

        private void SelectTheClickedItem(ListView list, int index)
        {
            for(int item = 0; item < list.Items.Count; item++)
            {
                if(item == index)
                {
                    list.Items[item].Selected = true;
                } else
                {
                    list.Items[item].Selected = false;
                }
            }
            
        }

        private void selectDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LoadDirectory();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (LoadedImages == null || LoadedImages.Count == 0)
            {
                MessageBox.Show("No images loaded to delete.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this image?", "Delete Image", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var imageToDelete = LoadedImages[SelectedImageIndex];
                var imagePathToDelete = $@"Downloads\{Guid.NewGuid()}.png"; // a temporary image path to delete
                imageToDelete.Save(imagePathToDelete, ImageFormat.Png);
                File.Delete(imagePathToDelete); // delete the temporary image file from the file system
                LoadedImages.RemoveAt(SelectedImageIndex); // remove the image from the LoadedImages list
                imageList.Items.RemoveAt(SelectedImageIndex); // remove the item from the ListView
                if (LoadedImages.Count == 0) // if no images are left, hide the UI elements
                {
                    imageList.Visible = false;
                    selectedImage.Visible = false;
                    nextBtn.Visible = false;
                    previousBtn.Visible = false;
                    saveAsBtn.Visible = false;
                    menuStrip1.Visible = false;
                    selectDirectoryBtn.Visible = true;
                }
                else // if there are still images left, update the SelectedImageIndex and display the next image
                {
                    if (SelectedImageIndex >= LoadedImages.Count) // if the deleted image was the last one, select the last remaining image
                    {
                        SelectedImageIndex = LoadedImages.Count - 1;
                    }
                    Image selectedImg = LoadedImages[SelectedImageIndex];
                    selectedImage.Image = selectedImg;
                    SelectTheClickedItem(imageList, SelectedImageIndex);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.LoadDirectory();
        }

        private void LoadDirectory()
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                // selected directory
                var selectedDirectory = folderBrowser.SelectedPath;
                // images paths from selected directory
                var imagePaths = Directory.GetFiles(selectedDirectory);
                // loading images from images paths
                LoadImagesFromFolder(imagePaths);

                // initializing images list
                ImageList images = new ImageList();
                images.ImageSize = new Size(130, 40);


                foreach (var image in LoadedImages)
                {
                    images.Images.Add(image);
                }

                // double check we have some images selected
                if(images.Images.Count > 0)
                {
                    imageList.Visible = true;
                    selectedImage.Visible = true;
                    nextBtn.Visible = true;
                    previousBtn.Visible = true;
                    saveAsBtn.Visible = true;
                    menuStrip1.Visible = true;
                    selectDirectoryBtn.Visible = false;

                }
                // setting our listview with the imagelist
                imageList.LargeImageList = images;
                

                for (int itemIndex = 1; itemIndex <= LoadedImages.Count; itemIndex++)
                {
                    imageList.Items.Add(new ListViewItem($"Image {itemIndex}", itemIndex - 1));
                }
            }
        }
    }
}
