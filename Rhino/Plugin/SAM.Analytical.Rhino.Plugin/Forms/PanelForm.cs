using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Rhino.Plugin
{
    public partial class PanelForm : Form
    {
        private List<Panel> panels;

        public PanelForm()
        {
            InitializeComponent();
        }

        public PanelForm(IEnumerable<Panel> panels)
        {
            InitializeComponent();

            if (panels != null)
            {
                this.panels = new List<Panel>(panels);
            }
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Button_Reset_Click(object sender, EventArgs e)
        {
            panels = panels?.ConvertAll(x => Create.Panel(x));

            Modify.SetWeights(panels);
            Modify.SetBucketSizes(panels);
            Modify.SetMaxExtends(panels);

            DialogResult = DialogResult.OK;
            Close();
        }

        public List<Panel> Panels
        {
            get
            {
                if(panels != null)
                {
                    for(int i = 0; i < panels.Count; i++)
                    {
                        if(panels[i] == null)
                        {
                            continue;
                        }

                        Panel panel = Create.Panel(panels[i]);

                        if(double.TryParse(TextBox_BucketSize.Text, out double bucketSize))
                        {
                            panel.SetValue(PanelParameter.BucketSize, bucketSize);
                        }
                        
                        if(panel.TryGetValue(PanelParameter.BucketSize, out bucketSize))
                        {
                            if(double.TryParse(TextBox_BucketSizeFactor.Text, out double factor))
                            {
                                panel.SetValue(PanelParameter.BucketSize, bucketSize * factor);
                            }
                        }


                        if (double.TryParse(TextBox_MaxExtend.Text, out double maxExtend))
                        {
                            panel.SetValue(PanelParameter.MaxExtend, maxExtend);
                        }

                        if (panel.TryGetValue(PanelParameter.MaxExtend, out maxExtend))
                        {
                            if (double.TryParse(TextBox_MaxExtendFactor.Text, out double factor))
                            {
                                panel.SetValue(PanelParameter.MaxExtend, maxExtend * factor);
                            }
                        }


                        if (double.TryParse(TextBox_Weight.Text, out double weight))
                        {
                            panel.SetValue(PanelParameter.Weight, weight);
                        }

                        if (panel.TryGetValue(PanelParameter.Weight, out weight))
                        {
                            if (double.TryParse(TextBox_WeightFactor.Text, out double factor))
                            {
                                panel.SetValue(PanelParameter.Weight, weight * factor);
                            }
                        }

                        panels[i] = panel;
                    }
                }
                
                return panels;
            }
        }

        private void PanelForm_Load(object sender, EventArgs e)
        {
            if(panels == null)
            {
                return;
            }

            HashSet<double> bucketSizes = new HashSet<double>();
            HashSet<double> MaxExtends = new HashSet<double>();
            HashSet<double> Weights = new HashSet<double>();
            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }
                
                if(panel.TryGetValue(PanelParameter.BucketSize, out double bucketSize))
                {
                    bucketSizes.Add(bucketSize);
                }
                else
                {
                    bucketSizes.Add(double.NaN);
                }

                if (panel.TryGetValue(PanelParameter.MaxExtend, out double maxExtend))
                {
                    MaxExtends.Add(maxExtend);
                }
                else
                {
                    MaxExtends.Add(double.NaN);
                }

                if (panel.TryGetValue(PanelParameter.Weight, out double weight))
                {
                    Weights.Add(weight);
                }
                else
                {
                    Weights.Add(double.NaN);
                }
            }

            if(bucketSizes.Count == 1 && !double.IsNaN(bucketSizes.First()))
            {
                TextBox_BucketSize.Text = bucketSizes.First().ToString();
            }

            if (MaxExtends.Count == 1 && !double.IsNaN(MaxExtends.First()))
            {
                TextBox_MaxExtend.Text = MaxExtends.First().ToString();
            }

            if (Weights.Count == 1 && !double.IsNaN(Weights.First()))
            {
                TextBox_Weight.Text = Weights.First().ToString();
            }

            TextBox_BucketSizeFactor.Text = (1.0).ToString();
            TextBox_MaxExtendFactor.Text = (1.0).ToString();
            TextBox_WeightFactor.Text = (1.0).ToString();
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
    }
}
