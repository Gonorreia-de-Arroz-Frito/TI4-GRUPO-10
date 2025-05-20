
#include <vector>
#include <iostream>

// Function to convert adjacency list to edge representation
std::vector<std::pair<int, int>> convert_to_edge_list(const std::vector<std::vector<int>> &adjacency_list)
{
    // Initialize an empty list to store the edges
    std::vector<std::pair<int, int>> edges;

    // Iterate through the adjacency list
    for (int vertex = 0; vertex < adjacency_list.size(); vertex++)
    {
        // Iterate through the vertices reachable from the current vertex
        for (int neighbor : adjacency_list[vertex])
        {
            // Add the edge (vertex, neighbor) to the edges list
            edges.emplace_back(vertex, neighbor);
        }
    }

    // Return the list of edges
    return edges;
}

int main()
{
    // Example adjacency list
    std::vector<std::vector<int>> adj_list = {{1, 2}, {0, 2, 3}, {0, 1, 4, 5}, {1}, {2}, {2}};

    // Convert the adjacency list to an edge list
    std::vector<std::pair<int, int>> edge_list = convert_to_edge_list(adj_list);

    // Print the adjacency list
    std::cout << "For the following Adjacency list:" << std::endl;
    for (int vertex = 0; vertex < adj_list.size(); vertex++)
    {
        std::cout << vertex << " : ";
        for (int neighbor : adj_list[vertex])
        {
            std::cout << neighbor << " ";
        }
        std::cout << std::endl;
    }

    // Print the edge list
    std::cout << "Below is the Edge list:" << std::endl;
    for (const auto &edge : edge_list)
    {
        std::cout << "(" << edge.first << ", " << edge.second << ")" << std::endl;
    }

    return 0;
}