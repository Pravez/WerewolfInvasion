using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// Tagging component for use with the LocalNavMeshBuilder
// Supports mesh-filter and terrain - can be extended to physics and/or primitives
[DefaultExecutionOrder(-200)]
public class NavMeshSourceTag : MonoBehaviour
{
    // Global containers for all active mesh/terrain tags
    /*public static List<MeshFilter> m_Meshes = new List<MeshFilter>();
    public static List<Terrain> m_Terrains = new List<Terrain>();*/
	public static List<MeshFilter> Meshes = new List<MeshFilter>();
	public static List<NavMeshModifierVolume> VolumeModifiers = new List<NavMeshModifierVolume>();
	public static int AgentTypeId;
    void OnEnable()
    {
		var volumes = GetComponents<NavMeshModifierVolume>();
		if(volumes != null)
			VolumeModifiers.AddRange(volumes);

		var modifier = GetComponent<NavMeshModifier>();
		if ((modifier != null) && (!modifier.AffectsAgentType(AgentTypeId) || (modifier.ignoreFromBuild) && modifier.AffectsAgentType(AgentTypeId)))
			return;

		var meshes = GetComponentsInChildren<MeshFilter>();
		if (meshes != null && meshes.Length > 0)
			Meshes.AddRange(meshes);
		/*var m = GetComponent<MeshFilter>();
        if (m != null)
        {
            m_Meshes.Add(m);
        }

        var t = GetComponent<Terrain>();
        if (t != null)
        {
            m_Terrains.Add(t);
        }*/
    }

    void OnDisable()
    {
		var volumes = GetComponents<NavMeshModifierVolume>();
		if (volumes != null)
		{
			for (int index = 0; index < volumes.Length; index++)
				VolumeModifiers.Remove(volumes[index]);
		}

		var modifier = GetComponent<NavMeshModifier>();
		if((modifier != null) && (modifier.ignoreFromBuild))
			return;

		var mesh = GetComponent<MeshFilter>();
		if(mesh != null)
			Meshes.Remove(mesh);
		/*
        var m = GetComponent<MeshFilter>();
        if (m != null)
        {
            m_Meshes.Remove(m);
        }

        var t = GetComponent<Terrain>();
        if (t != null)
        {
            m_Terrains.Remove(t);
        }*/
    }
	public static void CollectMeshes(ref List<NavMeshBuildSource> _sources)
	{
		_sources.Clear();
		for (var i = 0; i < Meshes.Count; ++i)
		{
			var mf = Meshes[i];

			if (mf == null)
				continue;

			var m = mf.sharedMesh;
			if (m == null)
				continue;

			var s = new NavMeshBuildSource();
			s.shape = NavMeshBuildSourceShape.Mesh;
			s.sourceObject = m;
			s.transform = mf.transform.localToWorldMatrix;
			var modifier = mf.GetComponent<NavMeshModifier>();
			s.area = modifier && modifier.overrideArea ? modifier.area : 0;
			_sources.Add(s);
		}
	}

	//----------------------------------------------------------------------------------------
	public static void CollectModifierVolumes(int _layerMask, ref List<NavMeshBuildSource> _sources)
	{
		foreach (var m in VolumeModifiers)
		{
			if ((_layerMask & (1 << m.gameObject.layer)) == 0)
				continue;
			if (!m.AffectsAgentType(AgentTypeId))
				continue;

			var mcenter = m.transform.TransformPoint(m.center);
			var scale = m.transform.lossyScale;
			var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

			var src = new NavMeshBuildSource();
			src.shape = NavMeshBuildSourceShape.ModifierBox;
			src.transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one);
			src.size = msize;
			src.area = m.area;
			_sources.Add(src);
		}
	}
    // Collect all the navmesh build sources for enabled objects tagged by this component
    /*public static void Collect(ref List<NavMeshBuildSource> sources)
    {
        sources.Clear();

        for (var i = 0; i < m_Meshes.Count; ++i)
        {
            var mf = m_Meshes[i];
            if (mf == null) continue;

            var m = mf.sharedMesh;
            if (m == null) continue;

            var s = new NavMeshBuildSource();
            s.shape = NavMeshBuildSourceShape.Mesh;
            s.sourceObject = m;
            s.transform = mf.transform.localToWorldMatrix;
            s.area = 0;
            sources.Add(s);
        }

        for (var i = 0; i < m_Terrains.Count; ++i)
        {
            var t = m_Terrains[i];
            if (t == null) continue;

            var s = new NavMeshBuildSource();
            s.shape = NavMeshBuildSourceShape.Terrain;
            s.sourceObject = t.terrainData;
            // Terrain system only supports translation - so we pass translation only to back-end
            s.transform = Matrix4x4.TRS(t.transform.position, Quaternion.identity, Vector3.one);
            s.area = 0;
            sources.Add(s);
        }
    }*/
}
