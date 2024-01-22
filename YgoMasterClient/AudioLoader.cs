//#define WITH_NAUDIO
//#define WITH_MINIMP3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
#if WITH_NAUDIO
using NAudio.Wave;
#endif

namespace YgoMasterClient
{
    static class AudioLoader
    {
        static object setLoaderTypeLocker = new object();
        static object initLoaderDllLocker = new object();
        static bool hasSetLoaderType;
        static bool hasInitedLoaderDll;
        static string audioLoaderDllPath;
        static Type audioLoaderType;

        static string[] supportedFormats = new string[0];
        public static string[] SupportedFormats
        {
            get
            {
                SetLoaderType();
                return supportedFormats;
            }
        }

        public static bool Available
        {
            get { return SupportedFormats != null; }
        }

        static void SetLoaderType()
        {
            lock (setLoaderTypeLocker)
            {
                if (!hasSetLoaderType)
                {
                    hasSetLoaderType = true;
                    string dll = Path.Combine(Program.CurrentDir, "bass.dll");
                    if (File.Exists(dll))
                    {
                        audioLoaderType = typeof(BassAudioLoader);
                    }
                    else
                    {
#if WITH_NAUDIO
                        dll = Path.Combine(Program.CurrentDir, "NAudio.dll");
                        if (File.Exists(dll))
                        {
                            audioLoaderType = typeof(NAudioLoader);
                        }
                        else
#endif
                        {
#if WITH_MINIMP3
                            audioLoaderType = typeof(MiniMp3Loader);
#endif
                        }
                    }
                    if (audioLoaderType == null)
                    {
                        return;
                    }
                    audioLoaderDllPath = dll;
                    supportedFormats = ((IAudioLoader)Activator.CreateInstance(audioLoaderType)).GetSupportedFormats();
                }
            }
        }

        public static IAudioLoader CreateInstance()
        {
            SetLoaderType();
            lock (initLoaderDllLocker)
            {
                if (!hasInitedLoaderDll && audioLoaderType != null)
                {
                    hasInitedLoaderDll = true;
                    ((IAudioLoader)Activator.CreateInstance(audioLoaderType)).Init(audioLoaderDllPath);
                }
                if (audioLoaderType == null)
                {
                    return null;
                }
            }
            return (IAudioLoader)Activator.CreateInstance(audioLoaderType);
        }

        class BassAudioLoader : IAudioLoader
        {
            static string[] supportedFormats = { ".mp3"/*, ".wav", ".ogg", ".aiff"*/ };

            [Flags]
            enum BASSInit
            {
                BASS_DEVICE_DEFAULT = 0,
            }

            [Flags]
            enum BASSFlag
            {
                BASS_SAMPLE_FLOAT = 256,
                BASS_STREAM_DECODE = 2097152,
                BASS_UNICODE = -2147483648,
            }

            [Flags]
            enum BASSChannelType
            {
            }

            [Flags]
            enum BASSMode
            {
                BASS_POS_BYTE = 0
            }

            [StructLayout(LayoutKind.Sequential)]
            struct BASS_CHANNELINFO_INTERNAL
            {
                public int freq;
                public int chans;
                public BASSFlag flags;
                public BASSChannelType ctype;
                public int origres;
                public int plugin;
                public int sample;
                public IntPtr filename;
            }

            [return: MarshalAs(UnmanagedType.Bool)]
            delegate bool Del_BASS_Init(int device, int freq, BASSInit flags, IntPtr win, IntPtr clsid);
            static Del_BASS_Init BASS_Init;

            delegate int Del_BASS_StreamCreateFileUnicode([MarshalAs(UnmanagedType.Bool)] bool mem, [MarshalAs(UnmanagedType.LPWStr)][In] string file, long offset, long length, BASSFlag flags);
            static Del_BASS_StreamCreateFileUnicode BASS_StreamCreateFileUnicode;

            [return: MarshalAs(UnmanagedType.Bool)]
            delegate bool Del_BASS_ChannelGetInfo(int handle, [In][Out] ref BASS_CHANNELINFO_INTERNAL info);
            static Del_BASS_ChannelGetInfo BASS_ChannelGetInfo;

            delegate long Del_BASS_ChannelGetLength(int handle, BASSMode mode);
            static Del_BASS_ChannelGetLength BASS_ChannelGetLength;

            delegate int Del_BASS_ChannelGetData(int handle, [In][Out] float[] buffer, int length);
            static Del_BASS_ChannelGetData BASS_ChannelGetData;

            [return: MarshalAs(UnmanagedType.Bool)]
            delegate bool Del_BASS_StreamFree(int handle);
            static Del_BASS_StreamFree BASS_StreamFree;

            int handle;

            public string[] GetSupportedFormats()
            {
                return supportedFormats;
            }

            public void Init(string dll)
            {
                IntPtr lib = PInvoke.LoadLibrary(dll);
                if (lib == IntPtr.Zero)
                {
                    return;
                }
                BASS_Init = (Del_BASS_Init)Marshal.GetDelegateForFunctionPointer(PInvoke.GetProcAddress(lib, "BASS_Init"), typeof(Del_BASS_Init));
                BASS_StreamCreateFileUnicode = (Del_BASS_StreamCreateFileUnicode)Marshal.GetDelegateForFunctionPointer(PInvoke.GetProcAddress(lib, "BASS_StreamCreateFile"), typeof(Del_BASS_StreamCreateFileUnicode));
                BASS_ChannelGetInfo = (Del_BASS_ChannelGetInfo)Marshal.GetDelegateForFunctionPointer(PInvoke.GetProcAddress(lib, "BASS_ChannelGetInfo"), typeof(Del_BASS_ChannelGetInfo));
                BASS_ChannelGetLength = (Del_BASS_ChannelGetLength)Marshal.GetDelegateForFunctionPointer(PInvoke.GetProcAddress(lib, "BASS_ChannelGetLength"), typeof(Del_BASS_ChannelGetLength));
                BASS_ChannelGetData = (Del_BASS_ChannelGetData)Marshal.GetDelegateForFunctionPointer(PInvoke.GetProcAddress(lib, "BASS_ChannelGetData"), typeof(Del_BASS_ChannelGetData));
                BASS_StreamFree = (Del_BASS_StreamFree)Marshal.GetDelegateForFunctionPointer(PInvoke.GetProcAddress(lib, "BASS_StreamFree"), typeof(Del_BASS_StreamFree));

                BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero, IntPtr.Zero);
            }

            public AudioInfo Open(string file)
            {
                handle = BASS_StreamCreateFileUnicode(false, file, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_UNICODE);
                if (handle == 0)
                {
                    return null;
                }

                BASS_CHANNELINFO_INTERNAL bassInfo = default(BASS_CHANNELINFO_INTERNAL);
                if (!BASS_ChannelGetInfo(handle, ref bassInfo))
                {
                    return null;
                }

                int lengthSamples = (int)BASS_ChannelGetLength(handle, BASSMode.BASS_POS_BYTE) / sizeof(float);
                return new AudioInfo(lengthSamples, bassInfo.freq, bassInfo.chans);
            }

            public int Read(float[] buffer)
            {
                int read = BASS_ChannelGetData(handle, buffer, buffer.Length * sizeof(float));
                read /= sizeof(float);
                return read;
            }

            public void Close()
            {
                if (handle != 0)
                {
                    BASS_StreamFree(handle);
                    handle = 0;
                }
            }
        }

#if WITH_NAUDIO
        class NAudioLoader : IAudioLoader
        {
            static string[] supportedFormats = { ".mp3"/*, ".wav", ".aiff"*/ };

            ISampleProvider sampleProvider;
            Stream reader;

            public string[] GetSupportedFormats()
            {
                return supportedFormats;
            }

            public void Init(string dll)
            {
            }

            public AudioInfo Open(string file)
            {
                WaveStream reader = null;
                switch (Path.GetExtension(file).ToLowerInvariant())
                {
                    case ".mp3":
                        reader = new Mp3FileReader(file);
                        break;
                    case ".wav":
                        reader = new WaveFileReader(file);
                        break;
                    case ".aiff":
                        reader = new AiffFileReader(file);
                        break;
                }

                this.reader = reader;
                sampleProvider = reader.ToSampleProvider();
                WaveFormat format = reader.WaveFormat;
                int lengthSamples = (int)reader.Length / (format.BitsPerSample / 8);
                return new AudioInfo(lengthSamples, format.SampleRate, format.Channels);
            }

            public int Read(float[] buffer)
            {
                return sampleProvider.Read(buffer, 0, buffer.Length);
            }

            public void Close()
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                    sampleProvider = null;
                }
            }
        }
#endif

#if WITH_MINIMP3
        class MiniMp3Loader : IAudioLoader
        {
            static string[] supportedFormats = { ".mp3" };
            static int structSize;

            IntPtr handle;

            public string[] GetSupportedFormats()
            {
                return supportedFormats;
            }

            public void Init(string dll)
            {
                structSize = PInvoke.lib_mp3dec_ex_t_sizeof();
            }

            public AudioInfo Open(string file)
            {
                handle = Marshal.AllocHGlobal(structSize);
                if (handle == IntPtr.Zero)
                {
                    return null;
                }

                Marshal.Copy(new byte[structSize], 0, handle, structSize);
                const int MP3D_SEEK_TO_SAMPLE = 1;
                if (PInvoke.lib_mp3dec_ex_open_w(handle, file, MP3D_SEEK_TO_SAMPLE) != 0)
                {
                    return null;
                }
                if (PInvoke.lib_mp3dec_ex_seek(handle, 0) != 0)
                {
                    return null;
                }
                ulong samples;
                int channels, hz;
                PInvoke.lib_mp3dec_ex_get_info(handle, out samples, out channels, out hz);
                return new AudioInfo((int)samples, hz, channels);
            }

            public int Read(float[] buffer)
            {
                return PInvoke.lib_mp3dec_ex_read(handle, buffer, buffer.Length);
            }

            public void Close()
            {
                if (handle != IntPtr.Zero)
                {
                    PInvoke.lib_mp3dec_ex_close(handle);
                    Marshal.FreeHGlobal(handle);
                    handle = IntPtr.Zero;
                }
            }
        }
#endif
    }

    class AudioInfo
    {
        public int LengthSamples { get; private set; }
        public int SampleRate { get; private set; }
        public int Channels { get; private set; }

        public AudioInfo(int lengthSamples, int sampleRate, int channels)
        {
            LengthSamples = lengthSamples;
            SampleRate = sampleRate;
            Channels = channels;
        }
    }

    interface IAudioLoader
    {
        string[] GetSupportedFormats();
        void Init(string dll);
        AudioInfo Open(string file);
        int Read(float[] buffer);
        void Close();
    }
}
