using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AngryBirdsGen
{
	public ShiftABLevel level;

	public AngryBirdsGen()
	{
		level = new ShiftABLevel();
	}

	public override int GetHashCode()
	{
		unchecked {
		    const int prime = 17;

			int hash = prime + level.birdsAmount;

			// get hash code for all items in array
			for(int i = 0; i < level.GetStacksAmount(); i++)
			{
				int subHash = 0;
			
				if(level.GetStack(i).Count > 0)
				
					for(LinkedListNode<ShiftABGameObject> obj1 = level.GetStack(i).First; obj1 != level.GetStack(i).Last.Next; obj1 = obj1.Next)
						subHash = subHash * 3 + obj1.Value.GetHashCode();
					
				hash = hash * prime + subHash;
			}

			return hash;
		}
	}

    public bool Equals(AngryBirdsGen otherGen)
   	{
        // if (ReferenceEquals(null, otherGen)) return false;
        // if (ReferenceEquals(this, otherGen)) return true;
		
		// the amount of birds must be the same
		if(level.birdsAmount != otherGen.level.birdsAmount)
			return false;

		// the amount of stacks must be the same
		if(level.GetTotalObjectsAmount() != otherGen.level.GetTotalObjectsAmount())
			return false;

		for(int i = 0; i < level.GetStacksAmount() && i < otherGen.level.GetStacksAmount(); i++)
		{
			// the height of each stack must be the same
			if(level.GetStack(i).Count != otherGen.level.GetStack(i).Count)
				return false;
			
			if(level.GetStack(i).Count > 0)
			{
				LinkedListNode<ShiftABGameObject> obj1 = level.GetStack(i).First;
				LinkedListNode<ShiftABGameObject> obj2 = otherGen.level.GetStack(i).First;

				for(; obj1 != level.GetStack(i).Last.Next && obj2 != otherGen.level.GetStack(i).Last.Next; obj1 = obj1.Next, obj2 = obj2.Next)
				{
					if(!obj1.Value.Equals(obj2.Value))
						return false;
				}
			}
		}

		return true;
   }
 
	public override bool Equals(object obj)
	{
	    // Since our other Equals() method already compares guys, we'll just call it.
	    // if (!(obj is AngryBirdsGen)) return false;
	    return Equals((AngryBirdsGen)obj);
	}

    public float HeightRatio()
    {
        float totalHeight = 0f;
        int numberOfStacks = level.GetStacksAmount();

        for(int i = 0; i < numberOfStacks; i++)
        {
            float stackHeight = 0f;

            LinkedList<ShiftABGameObject> stack = level.GetStack(i);
            for(LinkedListNode<ShiftABGameObject> node = stack.First; node != null; node = node.Next)
                stackHeight += node.Value.GetBounds().size.y;

            totalHeight += stackHeight;
        }

        float averageHeight = totalHeight / (float)numberOfStacks;

        return averageHeight / level.LevelPlayableHeight;
    }

    public float WidthHint()
    {
        int numberOfStacks = level.GetStacksAmount();
        if (numberOfStacks < 4)
            return 0f;
        if (numberOfStacks <6)
            return .5f;
        if (numberOfStacks > 10)
            return 0f;
        if (numberOfStacks > 7)
            return .5f;

        return 1f;
    }

    public float Complexity()
    {
        // Stop here if the level is empty
        if (level.GetStacksAmount() == 0)
            return 0f;

        // Total number of elements of all stacks used for normalization
        int nodeCount = 0;

        int complexityScore = 0;

        for (int i = 0; i < level.GetStacksAmount(); i++)
        {
            LinkedList<ShiftABGameObject> stack = level.GetStack(i);
            for(LinkedListNode<ShiftABGameObject> node = stack.First; node != null; node = node.Next)
            {
                nodeCount++;

                if(node.Value.ObjectsInside > 1)
                    complexityScore += node.Value.ObjectsInside;
            }
        }

        // Stop here if all stacks are empty
        if (nodeCount == 0)
            return 0f;

        float maxComplexityScore = nodeCount * level.GetStacksAmount();

        float complexity = (float) complexityScore / maxComplexityScore;

        return complexity;
    }

    public float Linearity()
    {
        return level.GetLevelLinearity();
    }

    public float Pattern(bool favorBigPatterns)
    {
        int maxPatternSize = MaxPatternSize();

        // a pattern consists of at least 2 blocks
        int minPatternSize = 1; 

        // Best pattern size found in the current level
        int bestPatternSize = minPatternSize;
        
        // Number of blocks that are part of the selected pattern in the current level
        int maxCoveredArea = 0;
        
        // Optimal pattern size if the current level was filled with the same pattern
        int optimalPatternSize = minPatternSize;

        // Number of blocks that would be part of the optimal pattern for the current level
        int maxOptimalCoveredArea = 0;

        int maxOccurrences = 0;

        for (int patternSize = minPatternSize; patternSize < maxPatternSize; patternSize++)
        {
            int optimalCoveredArea = 0;

            for(int i = 0; i < level.GetStacksAmount(); i++)
            {
                LinkedList<ShiftABGameObject> stack = level.GetStack(i);

                // Update optimalCoveredArea according to current stack
                // To do so, we calcute how many blocks a pattern with the current size would take at most
                int numberOfBlocks = stack.Count;
                optimalCoveredArea += numberOfBlocks - (numberOfBlocks % patternSize);

                // Calculate best covered area
                LinkedListNode<ShiftABGameObject> node = stack.First;
                while(node != null)
                {
                    int occurrences = PatternOccurrences(node, patternSize);
                    int coveredArea = occurrences * patternSize;

                    // If two pattern sizes give the same maximum covered area, we want either the minimun or maximun value (as passed as parameter)
                    if(coveredArea > 0 && coveredArea == maxCoveredArea)
                        bestPatternSize = favorBigPatterns ? Mathf.Max(bestPatternSize, patternSize) : Mathf.Min(bestPatternSize, patternSize);
                    // Otherwise keep looking for the maximum covered area
                    else if(coveredArea > maxCoveredArea)
                    {
                        maxCoveredArea = coveredArea;
                        bestPatternSize = patternSize;
                        maxOccurrences = occurrences;
                    }

                    node = node.Next;
                }
            }           
            
            // Calculate optimal covered area
            // If two pattern sizes give the same optimal covered area, we want either the minimun or maximun value (as passed as parameter)
            if(optimalCoveredArea == maxOptimalCoveredArea)
                optimalPatternSize = favorBigPatterns ? Mathf.Max(optimalPatternSize, patternSize) : Mathf.Min(optimalPatternSize, patternSize);
            else if(optimalCoveredArea > maxOptimalCoveredArea)
            {
                optimalPatternSize = patternSize;
                maxOptimalCoveredArea = optimalCoveredArea;
            }
        }
        
        float patternRatio = (float)maxCoveredArea / (float)maxOptimalCoveredArea;

        //Debug.Log("Occurrences : " + maxOccurrences + " Best size : " + bestPatternSize);

        return patternRatio;
    }

    private int MaxPatternSize()
    {
        int numberOfBlocksInBiggestStack = 0;
        for (int i = 0; i < level.GetStacksAmount(); i++)
            numberOfBlocksInBiggestStack = Mathf.Max(numberOfBlocksInBiggestStack, level.GetStack(i).Count);
        
        // A shape is a pattern if it appears at least twice
        // The maximum pattern size is half of that of the biggest stack
        // unless another stack has a number of blocks between the two previous values
        // in that case the maximum size is that of the latter stack
        int maxPatterSize = numberOfBlocksInBiggestStack / 2;
        bool ignoreFirstStackWithBiggestNumberOfBlock = true;
        
        for (int i = 0; i < level.GetStacksAmount(); i++)
        {
            int numberOfBlocks = level.GetStack(i).Count;
            if(numberOfBlocks == numberOfBlocksInBiggestStack && ignoreFirstStackWithBiggestNumberOfBlock)
            {
                ignoreFirstStackWithBiggestNumberOfBlock = false;
                continue;
            }
            
            maxPatterSize = Mathf.Max(maxPatterSize, numberOfBlocks);
        }
        
        return maxPatterSize;
    }

    private int PatternOccurrences(LinkedListNode<ShiftABGameObject> node, int patternSize)
    {
        int occurrences = 0;

        for (int i = 0; i < level.GetStacksAmount(); i++)
        {
            LinkedListNode<ShiftABGameObject> otherNode = level.GetStack(i).First;
            while(otherNode != null)
            {
                if(IsPatternMatching(node, otherNode, patternSize))
                   occurrences++;

                otherNode = otherNode.Next;
            }
        }

        return occurrences;
    }

    private bool IsPatternMatching(LinkedListNode<ShiftABGameObject> node, LinkedListNode<ShiftABGameObject> otherNode, int patternSize)
    {
        for (int i = 0; i < patternSize; i++)
        {
            if(node == null || otherNode == null || Overlap(node, otherNode, patternSize))
                return false;

            if(!node.Value.Equals(otherNode.Value))
                return false;
            
            node = node.Next;
            otherNode = otherNode.Next;
        }
        
        return true;
    }

    private bool Overlap(LinkedListNode<ShiftABGameObject> node, LinkedListNode<ShiftABGameObject> otherNode, int patternSize)
    {
        for (int i = 0; i < patternSize; i++)
        {
            if(node == null)
                break;

            if(node.Equals(otherNode))
               return true;

            node = node.Next;
        }

        return false;
    }
    
    public float PatternRatio()
    {
        int levelScore = 0;

        int minStackCount = int.MaxValue;
        for (int i = 0; i < level.GetStacksAmount(); i++)
        {
            if(level.GetStack(i).Count < minStackCount)
                minStackCount = level.GetStack(i).Count;
        }

        for (int i = 0; i < level.GetStacksAmount(); i++)
        {
            // Iteration on each node of all stacks
            for(LinkedListNode<ShiftABGameObject> node = level.GetStack(i).First; node != null; node = node.Next)
            {
                int bestScore = 0;

                // For each node, compare it with all other nodes in all other stacks
                for (int j = 0; j < level.GetStacksAmount(); j++)
                {
                    // We don't want to compare a stack with itself
                    if(i == j)
                        continue;

                    // Compare node with all other nodes in current stack j
                    for(LinkedListNode<ShiftABGameObject> otherNode = level.GetStack(j).First; otherNode != null; otherNode = otherNode.Next)
                    {
                        if(node.Value.Equals(otherNode.Value))
                        {
                            // Give 1 symmetry point if the same object exists in both stacks
                            int score = 1;
                            
                            // Check if the next objects also match
                            LinkedListNode<ShiftABGameObject> tempLeftNode = node.Next;
                            LinkedListNode<ShiftABGameObject> tempRightNode = otherNode.Next;
                            
                            while(tempLeftNode != null && tempRightNode != null)
                            {
                                if(tempLeftNode.Value.Equals(tempRightNode.Value))
                                {
                                    score *= 2;
                                    tempLeftNode = tempLeftNode.Next;
                                    tempRightNode = tempRightNode.Next;
                                }
                                // Stop as soon as both elements don't match
                                else 
                                    break;
                            }
                            
                            if(score > bestScore)
                                bestScore = score;
                        }
                    }
                }
                
                // Significant symmetry
                if(bestScore > 1)
                {
                    // If a symmetrical shape was found, do not consider the nodes that are part of it for the next iterations
                    for(int k = 0; k < bestScore; k++)
                    {
                        if(node != null)
                            node = node.Next;
                    }
                    
                    // Update total score 
                    levelScore += bestScore * bestScore;
                }
                else
                    node = node.Next;

                if(node == null)
                    break;
            }
        }

        float maxLevelScore = minStackCount;
        for (int i = 0; i < minStackCount; i++)
            maxLevelScore *= maxLevelScore;

        float patternRatio = (float)levelScore / maxLevelScore;

        return patternRatio;
    }

    public float Symmetry()
    {
        int stackCount = level.GetStacksAmount();
        
        // Stop here if there is only 1 stack (we don't want levels with only 1 stack)
        if (stackCount == 1)
            return 0f;

        bool hasPairNumberOfStacks = stackCount % 2 == 0;

        // If there is an odd number of stacks, the central stack will be ignored (always symmetrical to itself)
        int halfStackCount = hasPairNumberOfStacks ? stackCount / 2 : (stackCount - 1) / 2;

        int totalNumberOfBlocks = 0;
        int numberOfSymmetricBlocks = 0;

        for (int i = 0; i < halfStackCount; i++) 
        {
            LinkedList<ShiftABGameObject> leftStack = level.GetStack(i);
            totalNumberOfBlocks += leftStack.Count;

            LinkedList<ShiftABGameObject> rightStack = level.GetStack(stackCount - 1 - i);
            totalNumberOfBlocks += rightStack.Count;
            
            numberOfSymmetricBlocks += NumberOfSymmetricBlocs(leftStack, rightStack);
        }

        float symmetry = (float)numberOfSymmetricBlocks / (float)totalNumberOfBlocks;
        
        return symmetry;
    }

    private int NumberOfSymmetricBlocs(LinkedList<ShiftABGameObject> leftStack, LinkedList<ShiftABGameObject> rightStack)
    {
        int numberOfSymmetricBlocks = 0;
        int numberOfBlocksInLeftStack = leftStack.Count;
        int numberOfBlocksInRightStack = rightStack.Count;
        int minNumberOfBlocks = Mathf.Min(numberOfBlocksInLeftStack, numberOfBlocksInRightStack);

        LinkedListNode<ShiftABGameObject> leftNode = leftStack.First;
        LinkedListNode<ShiftABGameObject> rightNode = rightStack.First;

        for (int i = 0; i < minNumberOfBlocks; i++)
        {
            if(leftNode.Value.Equals(rightNode.Value))
               numberOfSymmetricBlocks += 2;

            leftNode = leftNode.Next;
            rightNode = rightNode.Next;
        }
        
        return numberOfSymmetricBlocks;
    }

	public float OldSymmetry()
    {
        int stackCount = level.GetStacksAmount();

        // Stop here if there is only 1 stack (we don't want levels with only 1 stack)
        if (stackCount == 1)
            return 0f;

        int halfStackCount = stackCount % 2 == 0 ? stackCount / 2 : (stackCount - 1) / 2;

        float symmetryScore = 0f;

		for (int i = 0; i < halfStackCount; i++) 
        {
            LinkedList<ShiftABGameObject> leftStack = level.GetStack(i);
            LinkedList<ShiftABGameObject> rightStack = level.GetStack(stackCount - 1 - i);

            symmetryScore += StackSymmetry(leftStack, rightStack);
		}

        float symmetry = symmetryScore / halfStackCount;

		return symmetry;
	}

    public float FlexSym()
    {
        int stackCount = level.GetStacksAmount();
        
        // Stop here if there is only 1 stack (we don't want levels with only 1 stack)
        if (stackCount == 1)
            return 0f;
        
        float symmetryScore = 0f;
        
        for (int i = 0; i < stackCount; i++) 
        {
            LinkedList<ShiftABGameObject> leftStack = level.GetStack(i);

            for(int j = 0; j < stackCount; j++)
            {
                if(i == j)
                    continue;

                LinkedList<ShiftABGameObject> rightStack = level.GetStack(stackCount - 1 - i);
                symmetryScore += StackSymmetry(leftStack, rightStack);
            }

        }

        float maxSymmetryScore = stackCount * (stackCount - 1);
        float symmetry = symmetryScore / maxSymmetryScore;
        
        return symmetry;
    }

    private float StackSymmetry(LinkedList<ShiftABGameObject> leftStack, LinkedList<ShiftABGameObject> rightStack)
    {
        // There is no symmetry if any of the stacks are empty
        if (leftStack.Count == 0 || rightStack.Count == 0)
            return 0f;

        float stackScore = 0;

        LinkedListNode<ShiftABGameObject> leftNode = leftStack.First;
        while (leftNode != null)
        {
            // The current left node will be compared with all right nodes
            // The best symmetry score for that left node will then be added to the stack score
            int bestScore = 0;

            // Compare current left node will all right nodes
            for(LinkedListNode<ShiftABGameObject> rightNode = rightStack.First; rightNode != null; rightNode = rightNode.Next)
            {
                if(leftNode.Value.Equals(rightNode.Value))
                {
                    // Give 1 symmetry point if the same object exists in both stacks
                    int score = 1;

                    // Check if the next objects also match
                    LinkedListNode<ShiftABGameObject> tempLeftNode = leftNode.Next;
                    LinkedListNode<ShiftABGameObject> tempRightNode = rightNode.Next;

                    while(tempLeftNode != null && tempRightNode != null)
                    {
                        if(tempLeftNode.Value.Equals(tempRightNode.Value))
                        {
                            score++;
                            tempLeftNode = tempLeftNode.Next;
                            tempRightNode = tempRightNode.Next;
                        }
                        // Stop as soon as both elements don't match
                        else 
                            break;
                    }

                    if(score > bestScore)
                        bestScore = score;
                }
            }
          
            // No significant symmetry, move on to next block
            if(bestScore == 0 || bestScore == 1)
                leftNode = leftNode.Next;
            else
            {
                //Debug.Log("leftLength: " + leftStack.Count + " rightlength: " + rightStack.Count + " score: " + bestScore);
                // If a symmetrical shape was found, do not consider the nodes that are part of it for the next iterations
                for(int i = 0; i < bestScore; i++)
                {
                    if(leftNode != null)
                        leftNode = leftNode.Next;
                }
                
                // Update total score 
                stackScore += bestScore * bestScore;
            }
        }

        int minLength = leftStack.Count;
        if (rightStack.Count < minLength)
            minLength = rightStack.Count;

        float maxStackScore = minLength * minLength;

        // Normalize stack score to [0,1]
        float symmetry = stackScore / maxStackScore;

        return symmetry;
    }

    private float StrictStackSymmetry(LinkedList<ShiftABGameObject> leftStack, LinkedList<ShiftABGameObject> rightStack)
    {
        // The maximum number of symmetrical items is the length of the shorter stack
        int maxSymmetricalElements = leftStack.Count;
        if (rightStack.Count < maxSymmetricalElements)
            maxSymmetricalElements = rightStack.Count;

        if (maxSymmetricalElements == 0)
            return 0f;
  
        int symmetricalElements = 0;

        LinkedListNode<ShiftABGameObject> leftObject = leftStack.First;
        LinkedListNode<ShiftABGameObject> rightObject = rightStack.First;

        for(; leftObject != null && rightObject != null; leftObject = leftObject.Next, rightObject = rightObject.Next)
        {
            if(leftObject.Value.Equals(rightObject.Value))
                symmetricalElements++;
        }

        // Calculate symmetry ratio
        float symmetry = symmetricalElements / maxSymmetricalElements;

        return symmetry;
    }
}