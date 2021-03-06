using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

namespace Ditho
{
    [ExecuteInEditMode]
    sealed class Warp : MonoBehaviour, ITimeControl, IPropertyPreview
    {
        #region Editable attributes

        [SerializeField] int _lineCount = 1000;

        [SerializeField] float _depth = 0.487f;
        [SerializeField] float _cutoff = 0.1f;
        [SerializeField] Vector3 _extent = Vector3.one;

        [SerializeField] float _speed = 1;
        [SerializeField, Range(0, 1)] float _speedRandomness = 0.5f;
        [SerializeField, Range(0, 0.5f)] float _length = 0.1f;
        [SerializeField, Range(0, 1)] float _lengthRandomness = 0.5f;

        [SerializeField, ColorUsage(false, true)] Color _lineColor = Color.white;
        [SerializeField, ColorUsage(false, true)] Color _sparkleColor = Color.white;
        [SerializeField, Range(0, 1)] float _sparkleDensity = 0.5f;

        [SerializeField] Shader _shader = null;

        void OnValidate()
        {
            _lineCount = Mathf.Max(_lineCount, 1);
        }

        #endregion

        #region Private members

        Mesh _mesh;
        Material _material;
        float _controlTime = -1;

        float LocalTime { get {
            if (_controlTime < 0)
                return Application.isPlaying ? Time.time : 0;
            else
                return _controlTime;
        } }

        void LazyInitialize()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.hideFlags = HideFlags.DontSave;
                _mesh.name = "Warp Effect";
                ReconstructMesh();
            }

            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }
        }

        #endregion

        #region Internal methods

        internal void ReconstructMesh()
        {
            _mesh.Clear();
            _mesh.vertices = new Vector3[_lineCount * 2];
            _mesh.SetIndices(
                Enumerable.Range(0, _lineCount * 2).ToArray(),
                MeshTopology.Lines, 0
            );
            _mesh.bounds = new Bounds(Vector3.zero, new Vector3(1, 1, 1000));
            _mesh.UploadMeshData(true);
        }

        #endregion

        #region ITimeControl implementation

        public void OnControlTimeStart()
        {
        }

        public void OnControlTimeStop()
        {
            _controlTime = -1;
        }

        public void SetTime(double time)
        {
            _controlTime = (float)time;
        }

        #endregion

        #region IPropertyPreview implementation

        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
        }

        #endregion

        #region MonoBehaviour implementation

        void OnDestroy()
        {
            Utility.Destroy(_mesh);
            Utility.Destroy(_material);
        }

        void LateUpdate()
        {
            LazyInitialize();

            _material.SetVector("_DepthParams", new Vector2(_depth, _cutoff));
            _material.SetVector("_Extent", _extent);

            _material.SetVector("_Speed", new Vector2(_speed, _speedRandomness));
            _material.SetVector("_Length", new Vector2(_length, _lengthRandomness));

            _material.SetColor("_LineColor", _lineColor);
            _material.SetColor("_SparkleColor", _sparkleColor);
            _material.SetFloat("_SparkleDensity", _sparkleDensity);

            _material.SetFloat("_LocalTime", LocalTime + 10);

            Graphics.DrawMesh(
                _mesh, transform.localToWorldMatrix,
                _material, gameObject.layer
            );
        }

        #endregion
    }
}
