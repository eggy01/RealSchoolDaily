using System.Collections.Generic;

[System.Serializable]
public class ForumData {
    public List<Section> sections;
}

[System.Serializable]
public class Section {
    public string sectionName;
    public List<Post> posts;
}

[System.Serializable]
public class Post {
    public string postTitle;
    public string postTime;
    public List<Reply> replies;
}

[System.Serializable]
public class Reply {
    public string floor;
    public string replyTo;
    public string username;
    public string level;
    public string content;
    public string avatar;
}