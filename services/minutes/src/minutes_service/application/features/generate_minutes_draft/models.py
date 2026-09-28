from dataclasses import dataclass
from datetime import datetime


@dataclass(frozen=True)
class DiscussionEntry:
    author_name: str
    content: str
    created_at: datetime
    proposal_title: str | None


@dataclass(frozen=True)
class Proposal:
    proposal_id: str
    title: str
    description: str | None


@dataclass(frozen=True)
class Topic:
    title: str
    proposals: tuple[Proposal, ...]
    discussion_entries: tuple[DiscussionEntry, ...]


@dataclass(frozen=True)
class MeetingContext:
    meeting_id: str
    title: str
    description: str | None
    participants: tuple[str, ...]
    topics: tuple[Topic, ...]


@dataclass(frozen=True)
class MinutesDraft:
    meeting_id: str
    completed_event_id: str
    content: str
    generated_at: datetime
