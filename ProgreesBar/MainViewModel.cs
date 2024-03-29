﻿using Microsoft.Win32;
using ProgreesBar.Command;
using ProgreesBar.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProgreesBar
{
    public class MainViewModel : NotificationService
    {
        public MainView MainView { get; set; }
        Thread thread;

        private double _maxLen;
        public double MaxLen
        {
            get { return _maxLen; }
            set
            {
                if (_maxLen != value)
                {
                    _maxLen = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _incValue;
        public double IncValue
        {
            get { return _incValue; }
            set
            {
                if (_incValue != value)
                {
                    _incValue = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand FromFileCommand { get; set; }
        public ICommand ToFileCommand { get; set; }
        public ICommand CopyCommand { get; set; }
        public ICommand SuspendCommand { get; set; }
        public ICommand ResumeCommand { get; set; }
        public ICommand AbortCommand { get; set; }

        public MainViewModel(MainView _MainView)
        {
            MainView = _MainView;
            MainView.PbFile.Foreground = Brushes.White;

            FromFileCommand = new RelayCommand(
                f =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "All Files|*.*";
                    openFileDialog.ShowDialog();
                    string filePath = openFileDialog.FileName;
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        MainView.FromFileBox.Text = filePath;
                    }
                },
                from => true);

            ToFileCommand = new RelayCommand(
                t =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "All Files|*.*";
                    openFileDialog.ShowDialog();
                    string filePath = openFileDialog.FileName;
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        MainView.ToFileBox.Text = filePath;
                    }
                },
                to => true);

            CopyCommand = new RelayCommand(
                c =>
                {
                    MainView.PbFile.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0BB70B"));
                    string sourceFilePath = MainView.ToFileBox.Text;
                    string destinationFilePath = MainView.FromFileBox.Text;

                    if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destinationFilePath))
                    {
                        MessageBox.Show("No From or To File");
                        return;
                    }
                    else
                    {
                        try
                        {
                            MainView.Suspend.IsEnabled = true;
                            MainView.Resume.IsEnabled = true;
                            MainView.Abort.IsEnabled = true;
                            thread = new Thread(() =>
                            {
                                using (FileStream stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                                {
                                    using (FileStream writeStream = new FileStream(destinationFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        var length = stream.Length;
                                        MaxLen = stream.Length;
                                        var readTemp = 10;
                                        var buffer = new byte[readTemp];
                                        do
                                        {
                                            var data = stream.Read(buffer, 0, readTemp);
                                            writeStream.Write(buffer, 0, readTemp);
                                            length -= readTemp;
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                IncValue += readTemp;

                                                MainView.PbFile.Value = IncValue;
                                            });
                                            Thread.Sleep(1000);
                                        } while (length > 0);
                                    }
                                }
                            });
                            thread.IsBackground = true;
                            thread.Start();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error");
                        }
                    }
                },
                c => true);

            SuspendCommand = new RelayCommand(
                suspend =>
                {
                    if (thread != null)
                        thread.Suspend();
                },
                s => true);

            ResumeCommand = new RelayCommand(
                resume =>
                {
                    if (thread != null)
                        thread.Resume();
                },
                r => true);

            AbortCommand = new RelayCommand(
                abort =>
                {
                    if (thread != null)
                    {
                        thread.Abort();
                    }
                },
                a => true);

        }
    }
}
