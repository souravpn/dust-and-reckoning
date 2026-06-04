using UnityEngine;

/// <summary>
/// Place this on any world object that is a discoverable clue.
/// Implements IInteractable so PlayerController can detect and interact with it.
///
/// On interact: registers the clue with EvidenceJournal, plays a brief
/// examine animation (placeholder), and shows the clue description.
///
/// Configure the ClueId to match the IDs in EvidenceJournal.ValidConnections.
/// </summary>
public class ClueObject : MonoBehaviour, IInteractable
{
    [Header("Clue")]
    [SerializeField] private string _clueId;
    [SerializeField] private string _displayName;
    [SerializeField] [TextArea(2,4)] private string _examineText;
    [SerializeField] private ClueType _clueType = ClueType.Document;

    [Header("Interaction")]
    [SerializeField] private bool _disappearAfterPickup = false;
    [SerializeField] private GameObject _highlightVisual;  // optional outline/glow

    private bool _hasBeenFound;

    // ── IInteractable ────────────────────────────────────────────────────

    public bool CanInteract() => !_hasBeenFound || !_disappearAfterPickup;

    public void OnHighlightEnter()
    {
        if (_highlightVisual != null)
            _highlightVisual.SetActive(true);
    }

    public void OnHighlightExit()
    {
        if (_highlightVisual != null)
            _highlightVisual.SetActive(false);
    }

    public void OnInteract(GameObject interactor)
    {
        if (string.IsNullOrEmpty(_clueId))
        {
            Debug.LogWarning($"[ClueObject] {name} has no ClueId set.");
            return;
        }

        // Register with journal
        EvidenceJournal.Instance?.DiscoverClue(_clueId);
        _hasBeenFound = true;

        // Log examine text to console (replace with UI popup in Milestone 2)
        Debug.Log($"[Examine] {_displayName}: {_examineText}");

        if (_disappearAfterPickup)
            gameObject.SetActive(false);
    }

    // ── Debug ─────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
