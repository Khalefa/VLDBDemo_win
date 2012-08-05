reset

set terminal postscript eps color enhanced
set output 'uk.eps'

#set ytics 0.25
#set format y "%.2f"

set multiplot


# Bigger plot options
#set yrange [-4:5]
set size 1,1
set origin 0,0
set title 'Two Years'
set xlabel 'time/s'
set ylabel 'variable/m'


### This is to plot the square. You can skip this ###
set arrow from 1.1,-0.9 to 1.0,0.3 lw 1 back filled
set arrow from 0.9,-3 to 1.5,-3 lw 1 front nohead
set arrow from 0.9,-1 to 1.5,-1 lw 1 front nohead
set arrow from 0.9,-1 to 0.9,-3 lw 1 front nohead
set arrow from 1.5,-1 to 1.5,-3 lw 1 front nohead
###################################

# This plots the big plot
plot "uk_org" using ($1):($2) title "" w l lt 1 lc 3 lw 3 
 

# Now we set the options for the smaller plot
set size 0.6,0.4
set origin 0.2,0.6
set title 'Zoom (Week)'
set xrange [1:336]
#set yrange [1:336]
set xlabel ""
set ylabel ""
unset arrow
set grid

# And finally let's plot the same set of data, but in the smaller plot
plot "uk_org" using ($1):($2) title "" w l lt 1 lc 3 lw 3 

# It's important to close the multiplot environment!!!
unset multiplot



