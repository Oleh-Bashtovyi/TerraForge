using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Utils;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Pools;

public class NodePool<TNode> where TNode : Node
{
    private readonly Logger<NodePool<TNode>> _logger = new();
    private readonly List<TNode> _pool;
    private readonly PackedScene _nodeScene;
    private int _currentIndex;

    public int PoolSize => _pool.Count;
    public int UsedNodes => _currentIndex;

    public NodePool(PackedScene nodeScene, int initPoolSize = 100)
    {
        _nodeScene = nodeScene;
        _pool = new(initPoolSize);
        _currentIndex = 0;

        var mockScene = _nodeScene.Instantiate() as TNode;
        if (mockScene == null)
        {
            var message = $"NodePool<{typeof(TNode)}>: Unable to instantiate node from scene: {nodeScene.ResourcePath}";
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        for (int i = 0; i < initPoolSize; i++)
        {
            var node = _nodeScene.Instantiate() as TNode; 
            _pool.Add(node);
        }
    }

    public TNode GetNode()
    {
        if (_currentIndex >= _pool.Count)
        {
            var newNode = _nodeScene.Instantiate() as TNode;
            _pool.Add(newNode);
            _currentIndex++;
            return newNode;
        }
        var node = _pool[_currentIndex];
        
        if (node.IsQueuedForDeletion())
        {
            _logger.LogError("Node was queued for deletion. Re-instantiating.");
            var newNode = _nodeScene.Instantiate() as TNode;
            _pool[_currentIndex] = newNode;
        }

        _currentIndex++;
        return node;
    }

    public void Reset()
    {
        _currentIndex = 0;
    }
}