﻿using System;
using System.Windows;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using System.Windows.Media.Animation;

namespace ExcitingVirtualPetCore
{
    public partial class MainWindow : Window
    {

        private IPet CurrentPet = null;
        private SaveFileDialog saveDialog;
        private OpenFileDialog openDialog;
        private Timer timer = new Timer();
        private ISimpleFactory factory;
        private Random rand = new Random();
        public MainWindow()
        {
            InitializeComponent();
            InitializePet();

            RefreshUI();
            SetEventHandlers();
            timer.InitializeFrames();
            timer.initialize(MainLoopTimer_Tick);



            saveDialog = new SaveFileDialog();
            openDialog = new OpenFileDialog();

            saveDialog.Filter = "pet files |*.pet";
            openDialog.Filter = "pet files |*.pet";
            saveDialog.DefaultExt = "pet files |*.pet";
            openDialog.DefaultExt = "pet files |*.pet";
        }
        private void SetEventHandlers()
        {
            CurrentPet.HungerChanged += UpdateView;
            CurrentPet.ThirstChanged += UpdateView;
            CurrentPet.BoredomChanged += UpdateView;
            CurrentPet.AffectionChanged += UpdateView;
            CurrentPet.WaterChanged += UpdateView;
            CurrentPet.FoodChanged += UpdateView;
            CurrentPet.SleepinessChanged += UpdateView;
        }


        private void MainLoopTimer_Tick(object sender, EventArgs e)
        {
            //increase frame counters
            timer.increaseNeedCounters();

            //update cat needs
            timer.updateFrames(CurrentPet);

            //check game loss condition
            petDisatisfied();

            checkSleep();
        }
        private void InitializePet()
        {

            factory = new Factory();
            CurrentPet = factory.CreateAnimal(rand.Next(1, 4));
            PetImage.Source = CurrentPet.currentImageState();
        }

        private void petDisatisfied()
        {
            //if you've really not taken care of your cat...
            if (CurrentPet.RanOff()) 
            { 
                PetImage.Source = CurrentPet.currentImageState();
                //stop main loop
                timer.stopTimer();
                MessageBox.Show("The pet ran away, choose another pet to play with for now.");
            }
            
        }

        private void checkSleep()
        {
            if (CurrentPet.IsSleeping()) 
            {
                startBlur();

                MessageBox.Show("Pet Is Asleep, return back later...");
                timer.stopTimer();

            }
        }

        //UNIQUE EXTRA FEATURE-----------------------------------------------------------------------------------------
        private void startBlur()
        {
            blurringEffect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(30, TimeSpan.FromSeconds(3)));
        }
        private void revertBlur()
        {
            blurringEffect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(3)));
        }

        
        private void UpdateView(object sender, EventArgs e)
        {
            HungerMeter.Value = CurrentPet.Hunger;
            ThirstMeter.Value = CurrentPet.Thirst;
            BoredomMeter.Value = CurrentPet.Boredom;
            AffectionMeter.Value = CurrentPet.Affection;
            WaterAmountBar.Value = CurrentPet.CurrentWater;
            FoodAmountBar.Value = CurrentPet.CurrentFood;
            SleepinessMeter.Value = CurrentPet.Sleepiness;
        }

        /*although this is sort of redundant due to the events handling this, 
        I think it's useful to include here because the UI is delayed to to the events only being triggered on changed, without this
        there's about a 3-4 second delay when you load the program/load a new pet, so this refresh is called to counter that */
        private void RefreshUI()
        {
            HungerMeter.Value = CurrentPet.Hunger;
            ThirstMeter.Value = CurrentPet.Thirst;
            BoredomMeter.Value = CurrentPet.Boredom;
            AffectionMeter.Value = CurrentPet.Affection;
            WaterAmountBar.Value = CurrentPet.CurrentWater;
            FoodAmountBar.Value = CurrentPet.CurrentFood;
            SleepinessMeter.Value = CurrentPet.Sleepiness;
        }
        private void PetFeedButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPet.feed();
        }

        private void PetWaterButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPet.water();
        }

        private void PetPlayButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPet.Play();

        }

        private void PetPatButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPet.pat();
        }

        private void Button_Load(object sender, RoutedEventArgs e)
        {
            if (openDialog.ShowDialog() == true)
            {
                using (Stream input = File.OpenRead(openDialog.FileName))
                using (BinaryReader reader = new BinaryReader(input))
                {

                    var options = new JsonSerializerOptions
                    {
                        Converters = { new PetClassConverter() },
                        WriteIndented = true

                    };

                    CurrentPet = JsonSerializer.Deserialize<Pet>(reader.ReadString(), options);

                    //You need to make sure you set the eventhandlers whenever you make a change to what object is currentPet. 
     
                    SetEventHandlers();
                    PetImage.Source = CurrentPet.currentImageState();
                    RefreshUI();
                    timer.startTimer();
                    revertBlur();

                }
            }
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            timer.stopTimer();
            if (saveDialog.ShowDialog() == true)
            {
                using (Stream output = File.Create(saveDialog.FileName))
                using (BinaryWriter writer = new BinaryWriter(output))

                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new PetClassConverter() },
                        WriteIndented = true
                    };


                    var jsonPet = JsonSerializer.Serialize(CurrentPet, CurrentPet.GetType(), options);
                    writer.Write(jsonPet);
                    Debug.WriteLine(jsonPet);
                    //timer.stopTimer();

                }
            }
        }
    }

 }

