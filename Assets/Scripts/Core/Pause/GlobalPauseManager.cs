using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GlobalPauseManager : MonoBehaviour, IPauseExempt
{
    [SerializeField] private Transform[] _gameplayRoots;
    [SerializeField] private bool _pauseAudioListener = false;
    [SerializeField] private bool _useTimeScale = false;
    [SerializeField] private bool _pauseGlobalPhysics = false;
    [SerializeField] private bool _pauseRigidbodies3D = true;
    [SerializeField] private bool _pauseRigidbodies2D = true;
    [SerializeField] private LayerMask _physicsAffectLayers = ~0;
    [SerializeField] private bool _disableBehaviours = true;
    [SerializeField] private PauseMask _pauseMaskToApply = PauseMask.Gameplay;

    private static GlobalPauseManager _instance;

    private IPauseService _pause;
    private CompositeDisposable _cd = new CompositeDisposable();

    private bool _savedAutoSim3D;
    private SimulationMode2D _savedSim2D;
    private bool _savedAudioListenerPause;
    private bool _globalsSnapshotted;
    private bool _globalsOverridden;
    private bool _suppressOnDestroyRestore;

    private readonly HashSet<MonoBehaviour> _disabledBehaviours = new HashSet<MonoBehaviour>();
    private readonly Dictionary<Animator, float> _animatorSpeeds = new Dictionary<Animator, float>();
    private readonly HashSet<ParticleSystem> _pausedParticles = new HashSet<ParticleSystem>();

    private struct RB3DState
    {
        public bool Kinematic;
        public bool Detect;
        public Vector3 Vel;
        public Vector3 AngVel;
    }

    private struct RB2DState
    {
        public bool Simulated;
        public Vector2 Vel;
        public float AngVel;
    }

    private readonly Dictionary<Rigidbody, RB3DState> _frozenRB3D = new Dictionary<Rigidbody, RB3DState>();
    private readonly Dictionary<Rigidbody2D, RB2DState> _frozenRB2D = new Dictionary<Rigidbody2D, RB2DState>();

    [Inject]
    public void Construct(IPauseService pause)
    {
        _pause = pause;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            _suppressOnDestroyRestore = true;
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _pause.ActiveMask.Subscribe(OnMaskChanged).AddTo(_cd);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        _cd.Dispose();

        if (!_suppressOnDestroyRestore && _globalsOverridden && _globalsSnapshotted)
        {
            RestoreGlobals();
        }

        _disabledBehaviours.Clear();
        _animatorSpeeds.Clear();
        _pausedParticles.Clear();
        _frozenRB3D.Clear();
        _frozenRB2D.Clear();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_pause.IsAnyActive(_pauseMaskToApply)) ApplyToNewContent();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        PruneDead();
    }

    private void OnMaskChanged(PauseMask mask)
    {
        bool active = (mask & _pauseMaskToApply) != 0;
        if (active) ApplyAll();
        else RestoreAll();
    }

    private void ApplyAll()
    {
        ApplyGlobalsIfNeeded();
        if (_useTimeScale) Time.timeScale = 0f;
        FreezeAnimators(false);
        PauseParticles(false);
        PauseRigidbodies3D(false);
        PauseRigidbodies2D(false);
        if (_disableBehaviours) DisableBehaviours(false);
    }

    private void RestoreAll()
    {
        if (_useTimeScale) Time.timeScale = 1f;
        RestoreAnimators();
        RestoreParticles();
        RestoreRigidbodies3D();
        RestoreRigidbodies2D();
        if (_disableBehaviours) RestoreBehaviours();
        RestoreGlobals();
    }

    private void ApplyToNewContent()
    {
        FreezeAnimators(true);
        PauseParticles(true);
        PauseRigidbodies3D(true);
        PauseRigidbodies2D(true);
        if (_disableBehaviours) DisableBehaviours(true);
    }

    private void ApplyGlobalsIfNeeded()
    {
        if (!_pauseGlobalPhysics && !_pauseAudioListener) return;
        if (!_globalsSnapshotted)
        {
            _savedAutoSim3D = Physics.autoSimulation;
            _savedSim2D = Physics2D.simulationMode;
            _savedAudioListenerPause = AudioListener.pause;
            _globalsSnapshotted = true;
        }
        if (_pauseGlobalPhysics)
        {
            Physics.autoSimulation = false;
            Physics2D.simulationMode = SimulationMode2D.Script;
        }
        if (_pauseAudioListener) AudioListener.pause = true;
        _globalsOverridden = true;
    }

    private void RestoreGlobals()
    {
        if (!_globalsOverridden || !_globalsSnapshotted) return;
        if (_pauseGlobalPhysics)
        {
            Physics.autoSimulation = _savedAutoSim3D;
            Physics2D.simulationMode = _savedSim2D;
        }
        if (_pauseAudioListener) AudioListener.pause = _savedAudioListenerPause;
        _globalsOverridden = false;
    }

    private void FreezeAnimators(bool incremental)
    {
        if (!incremental) _animatorSpeeds.Clear();
        if (_gameplayRoots == null || _gameplayRoots.Length == 0) return;

        for (int r = 0; r < _gameplayRoots.Length; r++)
        {
            Transform root = _gameplayRoots[r];
            if (root == null) continue;
            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator a = animators[i];
                if (a == null) continue;
                if (_animatorSpeeds.ContainsKey(a)) continue;
                _animatorSpeeds[a] = a.speed;
                a.speed = 0f;
            }
        }
    }

    private void RestoreAnimators()
    {
        foreach (KeyValuePair<Animator, float> kv in _animatorSpeeds)
        {
            if (kv.Key == null) continue;
            kv.Key.speed = kv.Value;
        }
        _animatorSpeeds.Clear();
    }

    private void PauseParticles(bool incremental)
    {
        if (!incremental) _pausedParticles.Clear();
        if (_gameplayRoots == null || _gameplayRoots.Length == 0) return;

        for (int r = 0; r < _gameplayRoots.Length; r++)
        {
            Transform root = _gameplayRoots[r];
            if (root == null) continue;
            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                if (ps == null) continue;
                if (ps.isPlaying)
                {
                    ps.Pause(true);
                    _pausedParticles.Add(ps);
                }
            }
        }
    }

    private void RestoreParticles()
    {
        foreach (ParticleSystem ps in _pausedParticles)
        {
            if (ps == null) continue;
            ps.Play(true);
        }
        _pausedParticles.Clear();
    }

    private bool LayerMatches(int layer)
    {
        return (_physicsAffectLayers.value & (1 << layer)) != 0;
    }

    private void PauseRigidbodies3D(bool incremental)
    {
        if (!_pauseRigidbodies3D) return;
        if (!incremental) _frozenRB3D.Clear();
        if (_gameplayRoots == null || _gameplayRoots.Length == 0) return;

        for (int r = 0; r < _gameplayRoots.Length; r++)
        {
            Transform root = _gameplayRoots[r];
            if (root == null) continue;
            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody rb = bodies[i];
                if (rb == null) continue;
                if (!LayerMatches(rb.gameObject.layer)) continue;
                if (_frozenRB3D.ContainsKey(rb)) continue;

                RB3DState st = new RB3DState
                {
                    Kinematic = rb.isKinematic,
                    Detect = rb.detectCollisions,
                    Vel = rb.linearVelocity,
                    AngVel = rb.angularVelocity
                };

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.detectCollisions = false;
                rb.isKinematic = true;

                _frozenRB3D[rb] = st;
            }
        }
    }

    private void RestoreRigidbodies3D()
    {
        foreach (KeyValuePair<Rigidbody, RB3DState> kv in _frozenRB3D)
        {
            if (kv.Key == null) continue;
            Rigidbody rb = kv.Key;
            RB3DState st = kv.Value;
            rb.isKinematic = st.Kinematic;
            rb.detectCollisions = st.Detect;
            rb.linearVelocity = st.Vel;
            rb.angularVelocity = st.AngVel;
        }
        _frozenRB3D.Clear();
    }

    private void PauseRigidbodies2D(bool incremental)
    {
        if (!_pauseRigidbodies2D) return;
        if (!incremental) _frozenRB2D.Clear();
        if (_gameplayRoots == null || _gameplayRoots.Length == 0) return;

        for (int r = 0; r < _gameplayRoots.Length; r++)
        {
            Transform root = _gameplayRoots[r];
            if (root == null) continue;
            Rigidbody2D[] bodies = root.GetComponentsInChildren<Rigidbody2D>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody2D rb = bodies[i];
                if (rb == null) continue;
                if (!LayerMatches(rb.gameObject.layer)) continue;
                if (_frozenRB2D.ContainsKey(rb)) continue;

                RB2DState st = new RB2DState
                {
                    Simulated = rb.simulated,
                    Vel = rb.linearVelocity,
                    AngVel = rb.angularVelocity
                };

                rb.simulated = false;

                _frozenRB2D[rb] = st;
            }
        }
    }

    private void RestoreRigidbodies2D()
    {
        foreach (KeyValuePair<Rigidbody2D, RB2DState> kv in _frozenRB2D)
        {
            if (kv.Key == null) continue;
            Rigidbody2D rb = kv.Key;
            RB2DState st = kv.Value;
            rb.simulated = st.Simulated;
            rb.linearVelocity = st.Vel;
            rb.angularVelocity = st.AngVel;
        }
        _frozenRB2D.Clear();
    }

    private void DisableBehaviours(bool incremental)
    {
        if (!_disableBehaviours) return;
        if (!incremental) _disabledBehaviours.Clear();
        if (_gameplayRoots == null || _gameplayRoots.Length == 0) return;

        for (int r = 0; r < _gameplayRoots.Length; r++)
        {
            Transform root = _gameplayRoots[r];
            if (root == null) continue;
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour b = behaviours[i];
                if (b == null) continue;
                if (ReferenceEquals(b, this)) continue;
                if (b is GlobalPauseManager) continue;
                if (b is IPauseExempt) continue;
                if (!b.enabled) continue;
                b.enabled = false;
                _disabledBehaviours.Add(b);
            }
        }
    }

    private void RestoreBehaviours()
    {
        foreach (MonoBehaviour b in _disabledBehaviours)
        {
            if (b == null) continue;
            b.enabled = true;
        }
        _disabledBehaviours.Clear();
    }

    private void PruneDead()
    {
        List<Animator> ra = null;
        foreach (KeyValuePair<Animator, float> kv in _animatorSpeeds)
        {
            if (kv.Key == null)
            {
                if (ra == null) ra = new List<Animator>();
                ra.Add(kv.Key);
            }
        }
        if (ra != null)
        {
            for (int i = 0; i < ra.Count; i++) _animatorSpeeds.Remove(ra[i]);
        }

        List<MonoBehaviour> rbm = null;
        foreach (MonoBehaviour b in _disabledBehaviours)
        {
            if (b == null)
            {
                if (rbm == null) rbm = new List<MonoBehaviour>();
                rbm.Add(b);
            }
        }
        if (rbm != null)
        {
            for (int i = 0; i < rbm.Count; i++) _disabledBehaviours.Remove(rbm[i]);
        }

        List<ParticleSystem> rps = null;
        foreach (ParticleSystem ps in _pausedParticles)
        {
            if (ps == null)
            {
                if (rps == null) rps = new List<ParticleSystem>();
                rps.Add(ps);
            }
        }
        if (rps != null)
        {
            for (int i = 0; i < rps.Count; i++) _pausedParticles.Remove(rps[i]);
        }

        List<Rigidbody> r3 = null;
        foreach (KeyValuePair<Rigidbody, RB3DState> kv in _frozenRB3D)
        {
            if (kv.Key == null)
            {
                if (r3 == null) r3 = new List<Rigidbody>();
                r3.Add(kv.Key);
            }
        }
        if (r3 != null)
        {
            for (int i = 0; i < r3.Count; i++) _frozenRB3D.Remove(r3[i]);
        }

        List<Rigidbody2D> r2 = null;
        foreach (KeyValuePair<Rigidbody2D, RB2DState> kv in _frozenRB2D)
        {
            if (kv.Key == null)
            {
                if (r2 == null) r2 = new List<Rigidbody2D>();
                r2.Add(kv.Key);
            }
        }
        if (r2 != null)
        {
            for (int i = 0; i < r2.Count; i++) _frozenRB2D.Remove(r2[i]);
        }
    }
}
