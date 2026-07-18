using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField]
    private string targetScene;

    [SerializeField]
    private string targetSpawnID;

    [Header("Objective Routing")]
    [SerializeField]
    private Transform compassTarget;

    public string TargetScene => targetScene;
    public Transform CompassTarget => compassTarget != null
        ? compassTarget
        : transform;

    private void OnTriggerEnter(
        Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneLoader.LoadScene(
            targetScene,
            targetSpawnID
        );
    }
}

public static class SceneRouteDatabase
{
    private const string ResourcePath = "SceneRoutes";
    private static SceneRouteCollection routes;

    public static string FindNextScene(
        string startScene,
        string destinationScene)
    {
        if (string.IsNullOrWhiteSpace(startScene) ||
            string.IsNullOrWhiteSpace(destinationScene) ||
            startScene == destinationScene)
        {
            return null;
        }

        EnsureLoaded();

        Queue<string> frontier = new();
        HashSet<string> visited = new() { startScene };
        Dictionary<string, string> previous = new();

        frontier.Enqueue(startScene);

        while (frontier.Count > 0)
        {
            string scene = frontier.Dequeue();

            foreach (string neighbor in GetNeighbors(scene))
            {
                if (!visited.Add(neighbor))
                    continue;

                previous[neighbor] = scene;

                if (neighbor == destinationScene)
                {
                    return ReconstructFirstStep(
                        startScene,
                        destinationScene,
                        previous);
                }

                frontier.Enqueue(neighbor);
            }
        }

        return null;
    }

    private static void EnsureLoaded()
    {
        if (routes != null)
            return;

        TextAsset routeData = Resources.Load<TextAsset>(ResourcePath);
        routes = routeData != null
            ? JsonUtility.FromJson<SceneRouteCollection>(routeData.text)
            : new SceneRouteCollection();
    }

    private static IEnumerable<string> GetNeighbors(string scene)
    {
        if (routes?.links == null)
            yield break;

        foreach (SceneRouteLink link in routes.links)
        {
            if (link == null)
                continue;

            if (link.fromScene == scene)
                yield return link.toScene;

            if (link.bidirectional && link.toScene == scene)
                yield return link.fromScene;
        }
    }

    private static string ReconstructFirstStep(
        string startScene,
        string destinationScene,
        Dictionary<string, string> previous)
    {
        string step = destinationScene;

        while (previous.TryGetValue(step, out string parent) &&
               parent != startScene)
        {
            step = parent;
        }

        return step;
    }
}

[Serializable]
public class SceneRouteCollection
{
    public List<SceneRouteLink> links = new();
}

[Serializable]
public class SceneRouteLink
{
    public string fromScene;
    public string toScene;
    public bool bidirectional = true;
}
