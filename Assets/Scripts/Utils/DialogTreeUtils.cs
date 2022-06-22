using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dialog;

public static class DialogTreeUtils
{

    // scriptable objects get modified in-game, we want to reset them
    public static void ClearVisitedFlag(Branch[] branches) {
        foreach (Branch branch in branches) {
            ClearVisitedFlag(branch.branches);
            branch.visited = false;
        }
    }

    public static void SetBranchParent(Branch[] branches, Branch parent) {
        foreach (Branch branch in branches) {
            branch.parent = parent;
            SetBranchParent(branch.branches, branch);
        }
    }

    public static void SetVisitedTree(Branch lastVisited) {
        bool isParentFullyVisited = true;
        if (lastVisited.parent != null) {
            foreach (Branch branch in lastVisited.parent.branches) {
                if (!branch.visited) {
                    isParentFullyVisited = false;
                }
            }
            if (isParentFullyVisited) {
                lastVisited.parent.visited = true;
                SetVisitedTree(lastVisited.parent);
            }
        }
    }

    public static Branch FindBranch(string option, Branch[] branches) {
        Branch targetBranch = null;
        foreach (Branch branch in branches) {
            if (branch.question == option) {
                targetBranch = branch;
            } else {
                Branch nextBranch = FindBranch(option, branch.branches);
                if (nextBranch != null) {
                    targetBranch = nextBranch;
                }
            }
        }
        return targetBranch;
    }


}
