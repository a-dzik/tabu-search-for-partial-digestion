### PDP MAX MATCH problem

The proposed algorithm is a **metaheuristic** solution for the **partial digestion problem**, which involves an experiment on a DNA molecule. Copies of the molecule in question are digested (cut) with a restriction enzyme in different time intervals. Consequently, the samples with longer digestion time will be more fragmented, and the ones with shorter exposure, will remain more intact. Ideally, this type of experiment would result in a multiset of all possible distances between the cutting sites (with "cutting sites" we also include both ends of the molecule). Based on this multiset, it is possible to reconstruct a map of all cuts (places where the enzyme cuts the molecule). So **the input is a multiset of distances, and the solution is the map of restriction sites**.

However, in reality, it is highly unlikely that we would get a perfectly correct multiset. Various versions of this problem assume different errors in the data - here we focus on one of them. The **MAX MATCH** variant of PDB corresponds to a situation, where some fragments were not correctly measured, resulting in substitutions in the multiset. Since no other types of errors are allowed in this variant, the multiset should always have \begin{pmatrix}
m \\
2
\end{pmatrix} elements, where m is the number of cuts + 2 ends of the molecule.

The **optimal solution** is a map, for which the corresponding multiset has a maximum amount of matching elements with the input multiset.


