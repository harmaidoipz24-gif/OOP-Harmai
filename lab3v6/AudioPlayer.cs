using System;

namespace OOP_Student
{
    public class AudioPlayer : IDisposable
    {
        private bool _disposed = false;
        private string _trackName;
        private bool _isPlaying;

        public string TrackName => _trackName;
        public bool IsPlaying => _isPlaying;

        public AudioPlayer(string trackName)
        {
            _trackName = trackName;
            _isPlaying = false;
            Console.WriteLine($"[Init] Ресурс для аудіотреку '{_trackName}' успішно виділено.");
        }

        public void Play()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _isPlaying = true;
            Console.WriteLine($"[AudioPlayer] Відтворення треку: '{_trackName}'");
        }

        public void Stop()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _isPlaying = false;
            Console.WriteLine($"[AudioPlayer] Зупинено: '{_trackName}'");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Звільнення керованих ресурсів
                    Console.WriteLine($"[Dispose(true)] Звільнення керованих ресурсів для '{_trackName}'.");
                }

                // Звільнення некерованих ресурсів (аудіо-дескрипторів)
                if (_isPlaying)
                {
                    _isPlaying = false;
                }
                Console.WriteLine($"[Dispose] Звільнення некерованого аудіо-ресурсу для '{_trackName}'.");

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AudioPlayer()
        {
            Console.WriteLine($"[~AudioPlayer] Деструктор викликано збирачем сміття для '{_trackName}'.");
            Dispose(false);
        }
    }
}